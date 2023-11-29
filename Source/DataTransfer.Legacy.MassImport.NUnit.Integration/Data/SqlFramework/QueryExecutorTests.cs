using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using kCura.Data.RowDataGateway;
using Moq;
using NUnit.Framework;
using Relativity.Logging;
using Relativity.MassImport.Data.SqlFramework;
using TimeoutException = kCura.Data.RowDataGateway.TimeoutException;

namespace MassImport.NUnit.Integration.Data.SqlFramework
{
	public class QueryExecutorTests : EmptyWorkspaceTestBase
	{
		private const string ExistingTableName = "[EDDSDBO].[ExistingTable]";

		private readonly ISqlQueryPart _selectOneQuery = new InlineSqlQuery($"SELECT 1;");
		private readonly ISqlQueryPart _selectTwoQuery = new InlineSqlQuery($"SELECT 2;");
		private readonly ISqlQueryPart _selectTextQuery = new InlineSqlQuery($"SELECT 'abc';");
		private readonly ISqlQueryPart _deleteFromExistingTableQuery = new InlineSqlQuery($"DELETE FROM {ExistingTableName};");
		private readonly ISqlQueryPart _deleteFromNonExistingTableQuery = new InlineSqlQuery($"DELETE FROM NotExistingTable;");

		private Mock<ILog> _loggerMock;
		private QueryExecutor _sut;

		[OneTimeSetUp]
		public Task OneTimeSetUp2Async()
		{
			var createTableQuery = new QueryInformation
			{
				Statement = $@"
			CREATE TABLE {ExistingTableName}(
				[ID] [int] NOT NULL
			);"
			};

			return EddsdboContext.ExecuteNonQueryAsync(createTableQuery);
		}

		[OneTimeTearDown]
		public Task OneTimeTear2DownAsync()
		{
			var deleteTableQuery = new QueryInformation
			{
				Statement = $"DROP TABLE {ExistingTableName};"
			};

			return EddsdboContext.ExecuteNonQueryAsync(deleteTableQuery);
		}

		[SetUp]
		public void SetUp()
		{
			_loggerMock = new Mock<ILog>();
			_sut = new QueryExecutor(this.EddsdboContext, _loggerMock.Object);
		}

		[Test]
		public void ExecuteBatchOfSqlStatementsAsScalar_ReturnsInt_WhenSingleStatementIsExecuted()
		{
			// arrange
			var query = _selectOneQuery;

			// act
			var result = this._sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query);

			// assert
			Assert.That(result, Is.EqualTo(1));
		}

		[Test]
		public void ExecuteBatchOfSqlStatementsAsScalar_ReturnsInt_WhenPassedAsParameter()
		{
			// arrange
			int expectedValue = 7;
			var query = new InlineSqlQuery($"SELECT @parameter;");
			var parameters = new[]
			{
				new SqlParameter("parameter", expectedValue)
			};

			// act
			var result = this._sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query, parameters);

			// assert
			Assert.That(result, Is.EqualTo(expectedValue));
		}

		[Test]
		public void ExecuteBatchOfSqlStatementsAsScalar_TimesOut_WhenQueryExecutesLongerThanTimeout()
		{
			// arrange
			var query = new InlineSqlQuery($"WAITFOR DELAY '00:00:02'");

			// act & assert
			Assert.That(
				() => this._sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query, timeoutInSeconds: 1),
				Throws.Exception.TypeOf<TimeoutException>());
		}

		[Test]
		public void ExecuteBatchOfSqlStatementsAsScalar_ReturnsString_WhenSingleStatementIsExecuted()
		{
			// arrange
			var query = _selectTextQuery;

			// act
			var result = this._sut.ExecuteBatchOfSqlStatementsAsScalar<string>(query);

			// assert
			Assert.That(result, Is.EqualTo("abc"));
		}

		[Test]
		public void ExecuteBatchOfSqlStatementsAsScalar_ThrowsExceptionAndLogError_WhenIncorrectTypeIsUsed()
		{
			// arrange
			var query = _selectTextQuery;

			// act & assert
			const string expectedExceptionMessage = "Error occurred when processing query result. Unable to convert scalar value to type 'System.Int32'.";
			const string expectedLogMessage = "Error occurred when processing {query} result. Unable to convert scalar value to type '{type}'.";
			Assert.That(
				() => this._sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query),
				Throws.Exception.TypeOf<FormatException>().With.Message.EqualTo(expectedExceptionMessage));
			_loggerMock.Verify(x => x.LogError(expectedLogMessage, It.IsAny<object[]>()));
		}

		[Test]
		public void ExecuteBatchOfSqlStatementsAsScalar_ReturnsValue_WhenMultipleStatementsAreExecutedAndSelectIsFirst()
		{
			// arrange
			var query = new SerialSqlQuery(_selectOneQuery, _deleteFromExistingTableQuery);

			// act
			var result = this._sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query);

			// assert
			Assert.That(result, Is.EqualTo(1));
		}

		[Test]
		public void ExecuteBatchOfSqlStatementsAsScalar_ReturnsValue_WhenMultipleStatementsAreExecutedAndSelectIsLast()
		{
			// arrange
			var query = new SerialSqlQuery(_deleteFromExistingTableQuery, _selectOneQuery);

			// act
			var result = this._sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query);

			// assert
			Assert.That(result, Is.EqualTo(1));
		}

		[Test]
		public void ExecuteBatchOfSqlStatementsAsScalar_LogsWarning_WhenBatchOfQueriesReturnsMoreThanOneValue()
		{
			// arrange
			var query = new SerialSqlQuery(_selectOneQuery, _selectTwoQuery);

			// act
			var result = this._sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query);

			// assert
			Assert.That(result, Is.EqualTo(1));
			const string expectedWarningMessage = "Query: {query} was executed as a scalar, but it returned more than one result.";
			_loggerMock.Verify(x => x.LogWarning(expectedWarningMessage, query.ToString()));
		}

		[Test]
		public void ExecuteBatchOfSqlStatementsAsScalar_ReturnsDefaultValueAndLogWarning_WhenQueryDoesNotReturnValue()
		{
			// arrange
			var query = _deleteFromExistingTableQuery;

			// act
			var result = this._sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query);

			// assert
			Assert.That(result, Is.EqualTo(0));
			const string expectedLogMessage = "Query: {query} was executed as a scalar, but it has not returned any value. Using default value for '{type}'.";
			_loggerMock.Verify(x => x.LogWarning(expectedLogMessage, It.IsAny<object[]>()));
		}

		[Test]
		public void ExecuteBatchOfSqlStatementsAsScalar_ThrowsError_WhenQueryBeforeSelectFails()
		{
			// arrange
			var query = new SerialSqlQuery(_deleteFromNonExistingTableQuery, _selectOneQuery);

			// act
			int ExecuteQueryAction() => this._sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query);

			// assert
			var expectedExceptionConstraint = Throws
				.Exception.TypeOf<ExecuteSQLStatementFailedException>()
				.With.Message.EqualTo("SQL Statement Failed")
				.And.InnerException.TypeOf<SqlException>()
				.And.InnerException.Message.EqualTo("Invalid object name 'NotExistingTable'.");
			Assert.That((Func<int>)ExecuteQueryAction, expectedExceptionConstraint);
		}

		[Test]
		public void ExecuteBatchOfSqlStatementsAsScalar_ThrowsError_WhenQueryAfterSelectFails()
		{
			// arrange
			var query = new SerialSqlQuery(_selectOneQuery, _deleteFromNonExistingTableQuery);

			// act
			int ExecuteQueryAction() => this._sut.ExecuteBatchOfSqlStatementsAsScalar<int>(query);

			// assert
			var expectedExceptionConstraint = Throws
				.Exception.TypeOf<ExecuteSQLStatementFailedException>()
				.With.Message.EqualTo("SQL Statement Failed")
				.And.InnerException.TypeOf<SqlException>()
				.And.InnerException.Message.EqualTo("Invalid object name 'NotExistingTable'.");
			Assert.That((Func<int>)ExecuteQueryAction, expectedExceptionConstraint);
		}
	}
}
