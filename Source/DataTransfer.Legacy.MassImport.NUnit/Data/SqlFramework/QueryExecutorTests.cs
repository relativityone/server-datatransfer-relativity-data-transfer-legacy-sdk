using System;
using System.Data;
using System.Data.SqlClient;
using kCura.Data.RowDataGateway;
using Moq;
using NUnit.Framework;
using Relativity.Logging;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.NUnit.Data.SqlFramework
{
	[TestFixture]
	public class QueryExecutorTests
	{
		private Mock<ILog> _loggerMock;
		private Mock<IBaseContextFacade> _baseContextMock;
		private Mock<IDataReader> _dataReaderMock;

		private QueryExecutor _sut;

		[SetUp]
		public void SetUp()
		{
			_loggerMock = new Mock<ILog>();
			_dataReaderMock = new Mock<IDataReader>();
			
			_baseContextMock = new Mock<IBaseContextFacade>();
			_baseContextMock
				.Setup(x => x.ExecuteSQLStatementAsReader(
					It.IsAny<string>(),
					It.IsAny<int>()))
				.Returns(_dataReaderMock.Object);
			_baseContextMock
				.Setup(x => x.ExecuteSQLStatementAsReader(
					It.IsAny<string>(),
					It.IsAny<SqlParameter[]>(),
					It.IsAny<int>()))
				.Returns(_dataReaderMock.Object);
			
			_sut = new QueryExecutor(_baseContextMock.Object, _loggerMock.Object);
		}

		[Test]
		public void ExecuteSQLStatementAsReader_CallsBaseContext_WithQuery()
		{
			// arrange
			const string queryAsString = "SQL QUERY";
			var query = new InlineSqlQuery($"{queryAsString}");

			// act
			_sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query);

			// assert
			const int expectedTimeout = -1;
			_baseContextMock.Verify(x => x.ExecuteSQLStatementAsReader(query.ToString(), expectedTimeout));
		}

		[Test]
		public void ExecuteSQLStatementAsReader_CallsBaseContext_WithQueryAndTimeout()
		{
			// arrange
			const string queryAsString = "SQL QUERY";
			var query = new InlineSqlQuery($"{queryAsString}");
			const int timeoutInSeconds = 4;

			// act
			_sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query, timeoutInSeconds);

			// assert
			_baseContextMock.Verify(x=>x.ExecuteSQLStatementAsReader(query.ToString(), timeoutInSeconds));
		}

		[Test]
		public void ExecuteSQLStatementAsReader_CallsBaseContext_WithQueryAndParameters()
		{
			// arrange
			const string queryAsString = "SQL QUERY";
			var query = new InlineSqlQuery($"{queryAsString}");
			SqlParameter[] parameters =
			{
				new SqlParameter("A", 2),
				new SqlParameter("B", "C")
			};

			// act
			_sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query, parameters);

			// assert
			const int expectedTimeout = -1;
			_baseContextMock.Verify(x => x.ExecuteSQLStatementAsReader(query.ToString(), parameters, expectedTimeout));
		}

		[Test]
		public void ExecuteSQLStatementAsReader_CallsBaseContext_WithQueryAndParametersAndTimeout()
		{
			// arrange
			const string queryAsString = "SQL QUERY";
			var query = new InlineSqlQuery($"{queryAsString}");
			const int timeoutInSeconds = 4;
			SqlParameter[] parameters =
			{
				new SqlParameter("A", 2),
				new SqlParameter("B", "C")
			};

			// act
			_sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query, parameters, timeoutInSeconds);

			// assert
			_baseContextMock.Verify(x => x.ExecuteSQLStatementAsReader(query.ToString(), parameters, timeoutInSeconds));
		}

		[Test]
		public void ExecuteSQLStatementAsReader_CallsBaseContext_WithQueryAsStringAndParameters()
		{
			// arrange
			const string queryAsString = "SQL QUERY";
			SqlParameter[] parameters =
			{
				new SqlParameter("A", 2),
				new SqlParameter("B", "C")
			};

			// act
			_sut.ExecuteBatchOfSqlStatementsAsScalar<int>(queryAsString, parameters);

			// assert
			const int expectedTimeout = -1;
			_baseContextMock.Verify(x => x.ExecuteSQLStatementAsReader(queryAsString, parameters, expectedTimeout));
		}

		[Test]
		public void ExecuteSQLStatementAsReader_CallsBaseContext_WithQueryAsStringAndParametersAndTimeout()
		{
			// arrange
			const string queryAsString = "SQL QUERY";
			const int timeoutInSeconds = 4;
			SqlParameter[] parameters =
			{
				new SqlParameter("A", 2),
				new SqlParameter("B", "C")
			};

			// act
			_sut.ExecuteBatchOfSqlStatementsAsScalar<int>(queryAsString, parameters, timeoutInSeconds);

			// assert
			_baseContextMock.Verify(x => x.ExecuteSQLStatementAsReader(queryAsString, parameters, timeoutInSeconds));
		}

		[TestCase(1, 1)]
		[TestCase(1.3, 1.3)]
		[TestCase(1, "1")]
		[TestCase("1", 1)]
		[TestCase("1.3", 1.3)]
		[TestCase("1", "1")]
		public void ExecuteSQLStatementAsReader_ConvertResult_WhenTypeIsValid<T>(object sqlResult, T expectedResult)
		{
			// arrange
			var query = new InlineSqlQuery($"SQL QUERY");
			_dataReaderMock.Setup(x => x.Read()).Returns(true);
			_dataReaderMock.Setup(x => x.GetValue(0)).Returns(sqlResult);

			// act
			T actualResult = _sut.ExecuteBatchOfSqlStatementsAsScalar<T>(query);

			// assert
			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[TestCase("abc", (int)1)]
		[TestCase("1.3", (int)1)]
		[TestCase(5_000_000_000, (int)1)]
		public void ExecuteSQLStatementAsReader_ThrowsException_WhenTypeIsInvalid<T>(object sqlResult, T requestedType)
		{
			// arrange
			var query = new InlineSqlQuery($"SQL QUERY");
			_dataReaderMock.Setup(x => x.Read()).Returns(true);
			_dataReaderMock.Setup(x => x.GetValue(0)).Returns(sqlResult);

			// act & assert
			Assert.That(()=> _sut.ExecuteBatchOfSqlStatementsAsScalar<T>(query), Throws.Exception.TypeOf<FormatException>());
			_loggerMock.Verify(x=>x.LogError(
				"Error occurred when processing {query} result. Unable to convert scalar value to type '{type}'.",
				query.ToString(),
				typeof(T)));
		}

		[Test]
		public void ExecuteSQLStatementAsReader_ReturnsDefault_WhenNoResult()
		{
			// arrange
			var query = new InlineSqlQuery($"SQL QUERY");
			_dataReaderMock.Setup(x => x.Read()).Returns(false);

			// act
			int actualResult = _sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query);

			// assert
			const int expectedResult = default;
			Assert.That(actualResult, Is.EqualTo(expectedResult));
			_loggerMock.Verify(x => x.LogWarning(
				"Query: {query} was executed as a scalar, but it has not returned any value. Using default value for '{type}'.",
				query.ToString(),
				typeof(int)));
		}

		[Test]
		public void ExecuteSQLStatementAsReader_ThrowsException_WhenBatchFailedBeforeReturningValue()
		{
			// arrange
			var expectedException = new Exception();

			var query = new InlineSqlQuery($"SQL QUERY");
			_dataReaderMock.Setup(x => x.Read()).Throws(expectedException);

			// act & assert
			Assert.That(
				() => _sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query),
				Throws.Exception.EqualTo(expectedException));
		}

		[Test]
		public void ExecuteSQLStatementAsReader_ThrowsException_WhenBatchFailedAfterReturningValue()
		{
			// arrange
			var expectedException = new Exception();

			var query = new InlineSqlQuery($"SQL QUERY");
			_dataReaderMock.Setup(x => x.Read()).Returns(true);
			_dataReaderMock.Setup(x => x.GetValue(0)).Returns(0);
			_dataReaderMock.
				SetupSequence(x => x.NextResult())
				.Returns(true)
				.Throws(expectedException);

			// act & assert
			Assert.That(
				() => _sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query), 
				Throws.Exception.TypeOf<ExecuteSQLStatementFailedException>());
		}

		[Test]
		public void ExecuteSQLStatementAsReader_LogsWarning_WhenMoreThanSingleResultReturned()
		{
			// arrange
			var query = new InlineSqlQuery($"SQL QUERY");
			_dataReaderMock.Setup(x => x.Read()).Returns(true);
			_dataReaderMock.Setup(x => x.GetValue(0)).Returns(0);
			_dataReaderMock.SetupSequence(x => x.NextResult()).Returns(true);

			// act
			_sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query);

			// assert
			_loggerMock.Verify(x => x.LogWarning(
				"Query: {query} was executed as a scalar, but it returned more than one result.",
				query.ToString()));
		}

		[Test]
		public void ExecuteSQLStatementAsReader_LogsWarning_WhenBatchFailedAfterReturningEmptyResult()
		{
			// arrange
			var expectedException = new Exception();

			var query = new InlineSqlQuery($"SQL QUERY");
			_dataReaderMock.Setup(x => x.Read()).Returns(false);
			_dataReaderMock.SetupSequence(x => x.NextResult()).Throws(expectedException);

			// act & assert
			Assert.That(
				() => _sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query),
				Throws.Exception.TypeOf<ExecuteSQLStatementFailedException>());
		}

		[Test]
		public void ExecuteSQLStatementAsReader_ReleasesConnection_BeforeItReturnsResult()
		{
			// arrange
			var query = new InlineSqlQuery($"SQL QUERY");
			_dataReaderMock.Setup(x => x.Read()).Returns(true);
			_dataReaderMock.Setup(x => x.GetValue(0)).Returns(1);

			// act
			_sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query);

			// assert
			_baseContextMock.Verify(x=>x.ReleaseConnection());
		}

		[Test]
		public void ExecuteSQLStatementAsReader_ReleasesConnection_BeforeItThrowsError()
		{
			// arrange
			var query = new InlineSqlQuery($"SQL QUERY");
			_dataReaderMock.Setup(x => x.Read()).Throws<Exception>();

			// act
			Assert.That(
				() => _sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query),
				Throws.Exception);

			// assert
			_baseContextMock.Verify(x=>x.ReleaseConnection());
		}
	}
}
