using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using kCura.Data.RowDataGateway;
using Moq;
using NUnit.Framework;
using Relativity.Logging;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.NUnit.Data.SqlFramework
{
	[TestFixture]
	public class AppLockTests
	{
		private Mock<BaseContext> _contextMock;
		private Mock<ILog> _loggerMock;

		[SetUp]
		public void SetUp()
		{
			_contextMock = new Mock<BaseContext>();
			_loggerMock = new Mock<ILog>();
		}

		[Test]
		public void ShouldThrowAnExceptionWhenTransactionIsNotActive()
		{
			// Act && Assert
			Assert.Throws<ArgumentException>(() =>
			{
				new AppLock(_contextMock.Object, "testResource", (c) => false, (c) => true, _loggerMock.Object);
			});
		}

		[Test]
		public void ShouldThrowAnExceptionWhenResourceNameIsEmpty()
		{
			// Act && Assert
			Assert.Throws<ArgumentException>(() =>
			{
				new AppLock(_contextMock.Object, "", (c) => true, (c) => true, _loggerMock.Object);
			});
		}

		[Test]
		public void ShouldThrowAnExceptionWhenTimeoutIsNegative()
		{
			// Act && Assert
			Assert.Throws<ArgumentException>(() =>
			{
				new AppLock(_contextMock.Object, "testResource", (c) => true, (c) => true, _loggerMock.Object , - 2);
			});
		}

		[Test]
		public void ShouldAcquireAndReleaseLock()
		{
			// Arrange
			_contextMock
				.Setup(m => m.ExecuteNonQuerySQLStatement(It.IsAny<string>(), It.IsAny<IEnumerable<SqlParameter>>()))
				.Callback<string, IEnumerable<SqlParameter>>(
					(sql, parameters) =>
					{
						parameters.ToArray()[1].Value = 0;
					});

			// Act 
			var applock = new AppLock(_contextMock.Object, "testResource", (c) => true, (c) => true, _loggerMock.Object);
			applock.Dispose();

			// Assert
			_contextMock.Verify(m => m.ExecuteNonQuerySQLStatement(It.IsAny<string>(), It.IsAny<IEnumerable<SqlParameter>>()), Times.Exactly(2));
		}

		[Test]
		public void ShouldNotReleaseLock()
		{
			// Arrange
			_contextMock
				.Setup(m => m.ExecuteNonQuerySQLStatement(It.IsAny<string>(), It.IsAny<IEnumerable<SqlParameter>>()))
				.Callback<string, IEnumerable<SqlParameter>>(
					(sql, parameters) =>
					{
						parameters.ToArray()[1].Value = 0;
					});

			// Act 
			var applock = new AppLock(_contextMock.Object, "testResource", (c) => true, (c) => false, _loggerMock.Object);
			applock.Dispose();

			// Assert
			_contextMock.Verify(m => m.ExecuteNonQuerySQLStatement(It.IsAny<string>(), It.IsAny<IEnumerable<SqlParameter>>()), Times.Once);
		}

		[Test]
		public void ShouldThrowAnExceptionWhenAcquiringLockEndsWithError([Values(-1, -2, -3, -4, -999)] int acquireLockReturnedValue)
		{
			// Arrange
			_contextMock
				.Setup(m => m.ExecuteNonQuerySQLStatement(It.IsAny<string>(), It.IsAny<IEnumerable<SqlParameter>>()))
				.Callback<string, IEnumerable<SqlParameter>>(
					(sql, parameters) =>
					{
						parameters.ToArray()[1].Value = acquireLockReturnedValue;
					});

			// Act && Assert
			Assert.Throws<AppLockException>(() =>
			{
				new AppLock(_contextMock.Object, "testResource", (c) => true, (c) => true, _loggerMock.Object);
			});
		}

		[Test]
		public void ShouldThrowAnExceptionWhenReleasingLockEndsWithException()
		{
			// Arrange
			int callsCount = 0;
			_contextMock
				.Setup(m => m.ExecuteNonQuerySQLStatement(It.IsAny<string>(), It.IsAny<IEnumerable<SqlParameter>>()))
				.Callback<string, IEnumerable<SqlParameter>>(
					(sql, parameters) =>
					{
						if (callsCount > 0)
						{
							throw new Exception("TestException");
						}
						parameters.ToArray()[1].Value = 0;
						callsCount++;
					});

			var applock = new AppLock(_contextMock.Object, "testResource", (c) => true, (c) => true, _loggerMock.Object);
			// Act && Assert
			Assert.Throws<AppLockException>(() =>
			{
				applock.Dispose();
			});
		}

		[Test]
		public void ShouldLogWarningWhenReleasingLockEndsWithError([Values(-1, -999)] int acquireLockReturnedValue)
		{
			// Arrange
			bool firstCall = true;

			_contextMock
				.Setup(m => m.ExecuteNonQuerySQLStatement(It.IsAny<string>(), It.IsAny<IEnumerable<SqlParameter>>()))
				.Callback<string, IEnumerable<SqlParameter>>(
					(sql, parameters) =>
					{
						parameters.ToArray()[1].Value = firstCall ? 0 : acquireLockReturnedValue;
						firstCall = false;
					});

			// Act 
			var applock = new AppLock(_contextMock.Object, "testResource", (c) => true, (c) => true, _loggerMock.Object);
			applock.Dispose();

			// Assert
			_loggerMock.Verify(m => m.LogWarning(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()));
		}
	}
}