using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using kCura.Utility;
using Moq;
using NUnit.Framework;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.NUnit.Data
{
	[TestFixture]
	public class BulkLoadSqlErrorRetryHelperTest
	{
		private const int _RETRY_COUNT = 3;
		private const int _RETRY_WAIT_TIME_IN_MILLISECONDS = 10;

		private Mock<Action> _actionMock;
		private Mock<IRetryLogger> _logger;

		private static List<int> SqlErrorsToRetry()
		{
			return new List<int>() { 4860, 4861 };
		}

		[SetUp()]
		public void SetUp()
		{
			_actionMock = new Mock<Action>();
			_logger = new Mock<IRetryLogger>();
		}

		[Test]
		public void SholdExecuteActionOnceWhenNoExceptionIsThrown()
		{
			BulkLoadSqlErrorRetryHelper.RetryOnBulkLoadSqlTemporaryError(_actionMock.Object, _RETRY_COUNT, _RETRY_WAIT_TIME_IN_MILLISECONDS, _logger.Object);
			_actionMock.Verify(action => action(), Times.Once());
		}

		[Test]
		public void SholdExecuteActionOnceAndThrowExceptionWhenNonSqlExceptionIsThrown()
		{
			_actionMock.Setup(action => action()).Throws(new ArgumentOutOfRangeException());

			Assert.Throws(typeof(ArgumentOutOfRangeException), () => BulkLoadSqlErrorRetryHelper.RetryOnBulkLoadSqlTemporaryError(_actionMock.Object, _RETRY_COUNT, _RETRY_WAIT_TIME_IN_MILLISECONDS, _logger.Object));

			_actionMock.Verify(action => action(), Times.Once());
		}

		[TestCase(1)]
		[TestCase(5001)]
		[TestCase(4862)]
		public void SholdExecuteActionOnceAndThrowWhenNotListedSqlExceptionIsThrown(int sqlErrorNumber)
		{
			_actionMock.Setup(action => action()).Throws(SqlExceptionCreator.NewSqlException(sqlErrorNumber));

			Assert.Throws(typeof(SqlException), () => BulkLoadSqlErrorRetryHelper.RetryOnBulkLoadSqlTemporaryError(_actionMock.Object, _RETRY_COUNT, _RETRY_WAIT_TIME_IN_MILLISECONDS, _logger.Object));

			_actionMock.Verify(action => action(), Times.Once());
		}

		[TestCaseSource(nameof(SqlErrorsToRetry))]
		public void SholdExecuteActionWithRetryAndThrowWhenListedSqlExceptionIsThrown(int sqlErrorNumber)
		{
			_actionMock.Setup(action => action()).Throws(SqlExceptionCreator.NewSqlException(sqlErrorNumber));

			Assert.Throws(typeof(SqlException), () => BulkLoadSqlErrorRetryHelper.RetryOnBulkLoadSqlTemporaryError(_actionMock.Object, _RETRY_COUNT, _RETRY_WAIT_TIME_IN_MILLISECONDS, _logger.Object));

			_actionMock.Verify(action => action(), Times.Exactly(_RETRY_COUNT));
		}

		[TestCaseSource(nameof(SqlErrorsToRetry))]
		public void SholdExecuteActionWithRetryAndNotThrowWhenLatterExecutionIsSuccessful(int sqlErrorNumber)
		{
			int numberOfCalls = 0;
			_actionMock.Setup(action => action()).Callback(() =>
			{
				numberOfCalls += 1;
				if (numberOfCalls < _RETRY_COUNT)
				{
					throw SqlExceptionCreator.NewSqlException(sqlErrorNumber);
				}
			});

			Assert.DoesNotThrow(() => BulkLoadSqlErrorRetryHelper.RetryOnBulkLoadSqlTemporaryError(_actionMock.Object, _RETRY_COUNT, _RETRY_WAIT_TIME_IN_MILLISECONDS, _logger.Object));

			_actionMock.Verify(action => action(), Times.Exactly(_RETRY_COUNT));
		}

		[Test]
		public void NoRetryIsLoggedWhenNoExceptionIsThrown()
		{
			BulkLoadSqlErrorRetryHelper.RetryOnBulkLoadSqlTemporaryError(_actionMock.Object, _RETRY_COUNT, _RETRY_WAIT_TIME_IN_MILLISECONDS, _logger.Object);

			_logger.Verify(logger => logger.LogRetry(It.IsAny<Exception>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
		}

		[TestCaseSource(nameof(SqlErrorsToRetry))]
		public void RetryIsLoggedAfterEachRetry(int sqlErrorNumber)
		{
			int numberOfCalls = 0;

			_actionMock.Setup(action => action()).Callback(() =>
			{
				numberOfCalls += 1;
				if (numberOfCalls < _RETRY_COUNT)
				{
					throw SqlExceptionCreator.NewSqlException(sqlErrorNumber);
				}

				_logger.Verify(logger => logger.LogRetry(It.IsAny<Exception>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(numberOfCalls - 1));
			});

			Assert.DoesNotThrow(() => BulkLoadSqlErrorRetryHelper.RetryOnBulkLoadSqlTemporaryError(_actionMock.Object, _RETRY_COUNT, _RETRY_WAIT_TIME_IN_MILLISECONDS, _logger.Object));

			_logger.Verify(logger => logger.LogRetry(It.IsAny<Exception>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(_RETRY_COUNT - 1));
		}

		[TestCaseSource(nameof(IsTooBigExceptionSetupVariablesList))]
		public void IsTooBigExceptionShouldBeFoundByFunction(IsTooBigExceptionSetupVariables variablesList)
		{
			Exception exception = SqlExceptionCreator.NewSqlException(variablesList.ErrorNumber);

			for (int index = 1, loopTo = variablesList.NestingLevel; index <= loopTo; index++)
			{
				exception = new Exception("some message", exception);
			}

			Assert.AreEqual(BulkLoadSqlErrorRetryHelper.IsTooMuchDataForSqlError(exception), variablesList.IsTooBigException);
		}

		public static List<IsTooBigExceptionSetupVariables> IsTooBigExceptionSetupVariablesList()
		{
			return new List<IsTooBigExceptionSetupVariables>()
			{
				new IsTooBigExceptionSetupVariables(1, 1, false), 
				new IsTooBigExceptionSetupVariables(1, 7119, true),
				new IsTooBigExceptionSetupVariables(0, 7119, true), 
				new IsTooBigExceptionSetupVariables(10, 7119, true),
				new IsTooBigExceptionSetupVariables(2, 7119, true), 
				new IsTooBigExceptionSetupVariables(1, 7118, false),
				new IsTooBigExceptionSetupVariables(6, 7120, false), 
				new IsTooBigExceptionSetupVariables(5, 100, false),
				new IsTooBigExceptionSetupVariables(2, 1, false), 
				new IsTooBigExceptionSetupVariables(19, 1, false)
			};
		}

		public class IsTooBigExceptionSetupVariables
		{
			public readonly int NestingLevel;
			public readonly int ErrorNumber;
			public readonly bool IsTooBigException;

			public IsTooBigExceptionSetupVariables(int nestingLevel, int errorNumber, bool isTooBigException)
			{
				NestingLevel = nestingLevel;
				ErrorNumber = errorNumber;
				IsTooBigException = isTooBigException;
			}
		}

		internal class SqlExceptionCreator
		{
			private static T Construct<T>(params object[] p)
			{
				var ctors = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
				return (T)ctors.First(ctor => ctor.GetParameters().Length == p.Length).Invoke(p);
			}

			internal static SqlException NewSqlException(int number = 1)
			{
				var collection = Construct<SqlErrorCollection>();
				var error = Construct<SqlError>(number, (byte)2, (byte)3, "server name", "error message", "proc", 100);
				var sqlErrorCollectonType = typeof(SqlErrorCollection);

				sqlErrorCollectonType.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(collection, new object[] { error });

				return typeof(SqlException).GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static, null, CallingConventions.ExplicitThis, new[] { typeof(SqlErrorCollection), typeof(string) }, new ParameterModifier[] { }).Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;
			}
		}
	}
}