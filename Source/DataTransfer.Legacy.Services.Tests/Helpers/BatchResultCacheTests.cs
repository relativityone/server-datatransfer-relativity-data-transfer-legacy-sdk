using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using Relativity.DataTransfer.Legacy.Services.SQL;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using FluentAssertions;
using Newtonsoft.Json;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers.BatchCache;
using Relativity.Services.Exceptions;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Helpers
{
	public class BatchResultCacheTests
	{
		private Mock<IAPILog> _loggerMock;
		private Mock<ISqlExecutor> _sqlExecutorMock;
		private IBatchResultCache _sut;

		private int WorkspaceID = 90210;

		[SetUp]
		public void SetUp()
		{
			_loggerMock = new Mock<IAPILog>();
			_sqlExecutorMock = new Mock<ISqlExecutor>();
			_sut = new BatchResultCache(_loggerMock.Object, _sqlExecutorMock.Object);
		}

		[Test]
		public void GetCreateOrThrow_ReturnsNull_WhenNewEntryIsCreated()
		{
			// Arrange
			string runID = Guid.NewGuid().ToString().Replace("-", "_");
			string batchID = Guid.NewGuid().ToString();

			_sqlExecutorMock.Setup(x => x.ExecuteReader(
					WorkspaceID,
					It.IsAny<string>(),
					It.IsAny<IEnumerable<SqlParameter>>(),
				It.IsAny<Func<IDataRecord, ResultCacheItem>>()))
				.Returns(new List<ResultCacheItem>
				{
					new ResultCacheItem(batchID, DateTime.Now, null, null, isNew: true),
				});

			// Act
			var result = _sut.GetCreateOrThrow(WorkspaceID, runID, batchID);

			// Assert
			result.Should().BeNull();
			_loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
			_loggerMock.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
		}

		[Test]
		public void GetCreateOrThrow_ReturnsNull_WhenSqlReturnsEmptyResults()
		{
			// Arrange
			string runID = Guid.NewGuid().ToString().Replace("-", "_");
			string batchID = Guid.NewGuid().ToString();

			_sqlExecutorMock.Setup(x => x.ExecuteReader(
					WorkspaceID,
					It.IsAny<string>(),
					It.IsAny<IEnumerable<SqlParameter>>(),
				It.IsAny<Func<IDataRecord, ResultCacheItem>>()))
				.Returns(new List<ResultCacheItem>());
			
			// Act
			var result = _sut.GetCreateOrThrow(WorkspaceID, runID, batchID);
			
			// Assert
			result.Should().BeNull();
			_loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
			_loggerMock.Verify(x => x.LogError(It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void GetCreateOrThrow_ReturnsNull_WhenSqlReturnsMoreThanOneResult()
		{
			// Arrange
			string runID = Guid.NewGuid().ToString().Replace("-", "_");
			string batchID = Guid.NewGuid().ToString();

			_sqlExecutorMock.Setup(x => x.ExecuteReader(
					WorkspaceID,
					It.IsAny<string>(),
					It.IsAny<IEnumerable<SqlParameter>>(),
				It.IsAny<Func<IDataRecord, ResultCacheItem>>()))
				.Returns(new List<ResultCacheItem>
				{
					new ResultCacheItem(batchID, DateTime.Now, null, null, isNew: true),
					new ResultCacheItem(batchID, DateTime.Now, null, null, isNew: false),
				});

			// Act
			var result = _sut.GetCreateOrThrow(WorkspaceID, runID, batchID);
			
			// Assert
			result.Should().BeNull();
			_loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
			_loggerMock.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
		}

		[Test]
		public void GetCreateOrThrow_ThrowException_WhenItemExistsButStillInProgress()
		{
			// Arrange
			string runID = Guid.NewGuid().ToString().Replace("-", "_");
			string batchID = Guid.NewGuid().ToString();

			_sqlExecutorMock.Setup(x => x.ExecuteReader(
					WorkspaceID,
					It.IsAny<string>(),
					It.IsAny<IEnumerable<SqlParameter>>(),
				It.IsAny<Func<IDataRecord, ResultCacheItem>>()))
				.Returns(new List<ResultCacheItem>
				{
					new ResultCacheItem(batchID, DateTime.Now.AddSeconds(-5), null, null, isNew: false),
				});

			// Act && Assert
			_sut.Invoking(x => x.GetCreateOrThrow(WorkspaceID, runID, batchID))
				.Should().Throw<ConflictException>().WithMessage("Batch In Progress");
			
			// Assert
			_loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
			_loggerMock.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
		}

		[Test]
		public void GetCreateOrThrow_ThrowBatchException_WhenItemExistsButTheResultIsEmpty()
		{
			// Arrange
			string runID = Guid.NewGuid().ToString().Replace("-", "_");
			string batchID = Guid.NewGuid().ToString();

			_sqlExecutorMock.Setup(x => x.ExecuteReader(
					WorkspaceID,
					It.IsAny<string>(),
					It.IsAny<IEnumerable<SqlParameter>>(),
				It.IsAny<Func<IDataRecord, ResultCacheItem>>()))
				.Returns(new List<ResultCacheItem>
				{
					new ResultCacheItem(batchID, DateTime.Now.AddSeconds(-5), DateTime.Now, null, isNew: false),
				});

			// Act && Assert
			_sut.Invoking(x => x.GetCreateOrThrow(WorkspaceID, runID, batchID))
				.Should().Throw<ServiceException>().WithMessage("Batch result is empty");
			
			// Assert
			_loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
			_loggerMock.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
		}

		[Test]
		public void GetCreateOrThrow_ThrowBatchException_WhenItemExistsButDeserializationFails()
		{
			// Arrange
			string runID = Guid.NewGuid().ToString().Replace("-", "_");
			string batchID = Guid.NewGuid().ToString();

			_sqlExecutorMock.Setup(x => x.ExecuteReader(
					WorkspaceID,
					It.IsAny<string>(),
					It.IsAny<IEnumerable<SqlParameter>>(),
					It.IsAny<Func<IDataRecord, ResultCacheItem>>()))
				.Returns(new List<ResultCacheItem>
				{
					new ResultCacheItem(batchID, DateTime.Now.AddSeconds(-5), DateTime.Now, "invalid serialization value", isNew: false),
				});

			// Act && Assert
			_sut.Invoking(x => x.GetCreateOrThrow(WorkspaceID, runID, batchID))
				.Should().Throw<ServiceException>().WithMessage("Failed to deserialize batch result").WithInnerException<JsonReaderException>();

			// Assert
			_loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
			_loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
		}

		[Test]
		public void GetCreateOrThrow_ReturnExistingResult_WhenItemExists()
		{
			// Arrange
			string runID = Guid.NewGuid().ToString().Replace("-", "_");
			string batchID = Guid.NewGuid().ToString();

			var expectedResult = new MassImportResults()
			{
				RunID = runID,
				ArtifactsCreated = 1,
				ArtifactsUpdated = 2,
				FilesProcessed = 3,
				ExceptionDetail = new SDK.ImportExport.V1.Models.SoapExceptionDetail(),
			};
			var serialized = JsonConvert.SerializeObject(expectedResult);

			_sqlExecutorMock.Setup(x => x.ExecuteReader(
					WorkspaceID,
					It.IsAny<string>(),
					It.IsAny<IEnumerable<SqlParameter>>(),
					It.IsAny<Func<IDataRecord, ResultCacheItem>>()))
				.Returns(new List<ResultCacheItem>
				{
					new ResultCacheItem(batchID, DateTime.Now.AddSeconds(-5), DateTime.Now, serialized, isNew: false),
				});

			// Act
			var result = _sut.GetCreateOrThrow(WorkspaceID, runID, batchID);

			// Assert
			result.Should().BeEquivalentTo(expectedResult);
			_loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
			_loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
		}

		[Test]
		public void Update_Return_WhenItemUpdated()
		{
			// Arrange
			string runID = Guid.NewGuid().ToString().Replace("-", "_");
			string batchID = Guid.NewGuid().ToString();

			var massImportResults = new MassImportResults()
			{
				RunID = runID,
				ArtifactsCreated = 1,
				ArtifactsUpdated = 2,
				FilesProcessed = 3,
				ExceptionDetail = new SDK.ImportExport.V1.Models.SoapExceptionDetail(),
			};

			_sqlExecutorMock.Setup(x => x.ExecuteNonQuerySQLStatement(
					WorkspaceID,
					It.IsAny<string>(),
					It.IsAny<IEnumerable<SqlParameter>>()))
				.Returns(1);

			// Act
			_sut.Update(WorkspaceID, runID, batchID, massImportResults);

			// Assert
			_loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
			_loggerMock.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
		}

		[Test]
		[TestCase(-1)]
		[TestCase(0)]
		[TestCase(2)]
		public void Update_ReturnAndLog_WhenItemNotUpdated(int numberOfRows)
		{
			// Arrange
			string runID = Guid.NewGuid().ToString().Replace("-", "_");
			string batchID = Guid.NewGuid().ToString();

			var massImportResults = new MassImportResults()
			{
				RunID = runID,
				ArtifactsCreated = 1,
				ArtifactsUpdated = 2,
				FilesProcessed = 3,
				ExceptionDetail = new SDK.ImportExport.V1.Models.SoapExceptionDetail(),
			};

			_sqlExecutorMock.Setup(x => x.ExecuteNonQuerySQLStatement(
					WorkspaceID,
					It.IsAny<string>(),
					It.IsAny<IEnumerable<SqlParameter>>()))
				.Returns(numberOfRows);

			// Act
			_sut.Update(WorkspaceID, runID, batchID, massImportResults);

			// Assert
			_loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
			_loggerMock.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
		}

		[Test]
		public void Cleanup_Return_WhenQueryExecuted()
		{
			// Arrange
			string runID = Guid.NewGuid().ToString().Replace("-", "_");

			_sqlExecutorMock.Setup(x => x.ExecuteNonQuerySQLStatement(
					WorkspaceID,
					It.IsAny<string>(),
					It.IsAny<IEnumerable<SqlParameter>>()))
				.Returns(1);

			// Act
			_sut.Cleanup(WorkspaceID, runID);

			// Assert
			_loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
			_loggerMock.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
		}

		[Test]
		public void GetResult_ReturnsNull_WhenSqlReturnsEmptyResults()
		{
			// Arrange
			string runID = Guid.NewGuid().ToString().Replace("-", "_");

			_sqlExecutorMock.Setup(x => x.ExecuteReader(
					WorkspaceID,
					It.IsAny<string>(),
					It.IsAny<IEnumerable<SqlParameter>>(),
					It.IsAny<Func<IDataRecord, ResultCacheItem>>()))
				.Returns(new List<ResultCacheItem>());

			// Act
			var result = _sut.GetResult(WorkspaceID, runID);

			// Assert
			result.Should().BeNull();
			_loggerMock.Verify(x => x.LogWarning(It.IsAny<string>()), Times.Once);
		}

		[Test]
		public void GetResult_ReturnExistingResult_WhenItemExists()
		{
			// Arrange
			string runID = Guid.NewGuid().ToString().Replace("-", "_");
			string batchID = Guid.NewGuid().ToString();

			var expectedResult = new MassImportResults()
			{
				RunID = runID,
				ArtifactsCreated = 100,
				ArtifactsUpdated = 4,
				FilesProcessed = 3,
				ExceptionDetail = new SDK.ImportExport.V1.Models.SoapExceptionDetail(),
			};
			var serialized = JsonConvert.SerializeObject(expectedResult);

			_sqlExecutorMock.Setup(x => x.ExecuteReader(
					WorkspaceID,
					It.IsAny<string>(),
					It.IsAny<IEnumerable<SqlParameter>>(),
					It.IsAny<Func<IDataRecord, ResultCacheItem>>()))
				.Returns(new List<ResultCacheItem>
				{
					new ResultCacheItem(batchID, DateTime.Now.AddSeconds(-5), DateTime.Now, serialized, isNew: false),
				});

			// Act
			var result = _sut.GetResult(WorkspaceID, runID);

			// Assert
			result.Should().BeEquivalentTo(expectedResult);
			_loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
			_loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
		}

		[Test]
		public void GetGet_ReturnsNull_WhenItemExistsButStillInProgress()
		{
			// Arrange
			string runID = Guid.NewGuid().ToString().Replace("-", "_");
			string batchID = Guid.NewGuid().ToString();

			DateTime? finishedOn = null;

			_sqlExecutorMock.Setup(x => x.ExecuteReader(
					WorkspaceID,
					It.IsAny<string>(),
					It.IsAny<IEnumerable<SqlParameter>>(),
					It.IsAny<Func<IDataRecord, ResultCacheItem>>()))
				.Returns(new List<ResultCacheItem>
				{
					new ResultCacheItem(batchID, DateTime.Now.AddSeconds(-5), finishedOn, null, isNew: false),
				});

			// Act
			var result = _sut.GetResult(WorkspaceID, runID);

			// Assert
			result.Should().Be(null);
			_loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
		}
	}
}
