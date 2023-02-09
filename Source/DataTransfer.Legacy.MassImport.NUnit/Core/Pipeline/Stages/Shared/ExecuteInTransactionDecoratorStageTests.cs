using System;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using Moq;
using NUnit.Framework;
using Relativity.Core;
using Relativity.Logging;
using Relativity.MassImport.Core.Pipeline;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Stages.Shared;
using Relativity.API;

namespace Relativity.MassImport.NUnit.Core.Pipeline.Stages.Shared
{
	[TestFixture]
	public class ExecuteInTransactionDecoratorStageTests
	{
		private Mock<IPipelineStage<int>> _stageMock;
		private Mock<ILog> _loggerMock;
		private Mock<kCura.Data.RowDataGateway.BaseContext> _dbContextMock;
		private Mock<BaseContext> _baseContextMock;
		private ExecuteInTransactionDecoratorStage<int, int> _sut;

		[SetUp]
		public void SetUp()
		{
			var pipelineExecutor = new PipelineExecutor();
			_stageMock = new Mock<IPipelineStage<int>>();
			_loggerMock = new Mock<ILog>();
			_loggerMock
				.Setup(x => x.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
				.Returns(_loggerMock.Object);

			_dbContextMock = new Mock<kCura.Data.RowDataGateway.BaseContext>();
			_baseContextMock = new Mock<BaseContext>();
			_baseContextMock.Setup(x => x.DBContext).Returns(_dbContextMock.Object);

			var massImportContext = new MassImportContext(
				_baseContextMock.Object,
				new LoggingContext("correlationId", "clientName", _loggerMock.Object),
				jobDetails: null, // not used by ExecuteInTransactionDecoratorStage
				caseSystemArtifactId: -1, // not used by ExecuteInTransactionDecoratorStage
				new Mock<IHelper>().Object
			);

			_sut = ExecuteInTransactionDecoratorStage.New(_stageMock.Object, pipelineExecutor, massImportContext);
		}

		[Test]
		public void ShouldBeginAndCommitTransactionWhenStageSucceeded()
		{
			// arrange
			const int expectedResult = 3;

			SetupTransaction(isOpen: false);
			_stageMock
				.Setup(x => x.Execute(It.IsAny<int>()))
				.Returns(expectedResult);

			// act
			int actualResult = _sut.Execute(5);

			// assert
			_baseContextMock.Verify(x => x.BeginTransaction(), Times.Once);
			_baseContextMock.Verify(x => x.CommitTransaction(), Times.Once);
			_baseContextMock.Verify(x => x.RollbackTransaction(), Times.Never);

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ShouldBeginAndRollbackTransactionWhenStageFailed()
		{
			// arrange
			var expectedException = new ArgumentNullException();

			SetupTransaction(isOpen: false);
			_stageMock
				.Setup(x => x.Execute(It.IsAny<int>()))
				.Throws(expectedException);

			// act & assert
			Assert.That(() => _sut.Execute(5), Throws.Exception.EqualTo(expectedException));

			// assert
			_baseContextMock.Verify(x => x.BeginTransaction(), Times.Once);
			_baseContextMock.Verify(x => x.CommitTransaction(), Times.Never);
			_baseContextMock.Verify(x => x.RollbackTransaction(), Times.Once);
		}

		[Test]
		public void ShouldThrowOriginalExceptionWhenRollbackFailed()
		{
			// arrange
			var expectedException = new ArgumentNullException();
			var rollbackException = new InvalidOperationException();

			SetupTransaction(isOpen: false);
			_stageMock
				.Setup(x => x.Execute(It.IsAny<int>()))
				.Throws(expectedException);
			_baseContextMock
				.Setup(x => x.RollbackTransaction())
				.Throws(rollbackException);

			// act & assert
			Assert.That(() => _sut.Execute(5), Throws.Exception.EqualTo(expectedException));

			// assert
			_baseContextMock.Verify(x => x.BeginTransaction(), Times.Once);
			_baseContextMock.Verify(x => x.CommitTransaction(), Times.Never);
			_baseContextMock.Verify(x => x.RollbackTransaction(), Times.Once);

			_loggerMock.Verify(x => x.LogError(rollbackException, "Exception occured when rolling back a transaction."));
		}

		[Test]
		public void ShouldThrowExceptionAndRollbackWhenCommitFailed()
		{
			// arrange
			const int expectedResult = 3;
			var commitException = new InvalidOperationException();

			SetupTransaction(isOpen: false);
			_stageMock
				.Setup(x => x.Execute(It.IsAny<int>()))
				.Returns(expectedResult);
			_baseContextMock
				.Setup(x => x.CommitTransaction())
				.Throws(commitException);

			// act & assert
			Assert.That(() => _sut.Execute(5), Throws.Exception.EqualTo(commitException));

			// assert
			_baseContextMock.Verify(x => x.BeginTransaction(), Times.Once);
			_baseContextMock.Verify(x => x.CommitTransaction(), Times.Once);
			_baseContextMock.Verify(x => x.RollbackTransaction(), Times.Once);

			_loggerMock.Verify(x => x.LogError(commitException, "Exception occured when commiting a transaction. Trying to rollback."));
		}

		[Test]
		public void ShouldThrowCommitExceptionWhenCommitAndRollbackFailed()
		{
			// arrange
			const int expectedResult = 3;
			var commitException = new InvalidOperationException();
			var rollbackException = new ArgumentNullException();

			SetupTransaction(isOpen: false);
			_stageMock
				.Setup(x => x.Execute(It.IsAny<int>()))
				.Returns(expectedResult);
			_baseContextMock
				.Setup(x => x.CommitTransaction())
				.Throws(commitException);
			_baseContextMock
				.Setup(x => x.RollbackTransaction())
				.Throws(rollbackException);

			// act & assert
			Assert.That(() => _sut.Execute(5), Throws.Exception.EqualTo(commitException));

			// assert
			_baseContextMock.Verify(x => x.BeginTransaction(), Times.Once);
			_baseContextMock.Verify(x => x.CommitTransaction(), Times.Once);
			_baseContextMock.Verify(x => x.RollbackTransaction(), Times.Once);

			_loggerMock.Verify(x => x.LogError(commitException, "Exception occured when commiting a transaction. Trying to rollback."));
			_loggerMock.Verify(x => x.LogError(rollbackException, "Exception occured when rolling back a transaction."));
		}

		[Test]
		public void ShouldNotManageTransactionWhenStageSucceededAndTransactionWasAlreadyOpen()
		{
			// arrange
			const int expectedResult = 3;

			SetupTransaction(isOpen: true);
			_stageMock
				.Setup(x => x.Execute(It.IsAny<int>()))
				.Returns(expectedResult);

			// act
			int actualResult = _sut.Execute(5);

			// assert
			_baseContextMock.Verify(x => x.BeginTransaction(), Times.Never);
			_baseContextMock.Verify(x => x.CommitTransaction(), Times.Never);
			_baseContextMock.Verify(x => x.RollbackTransaction(), Times.Never);

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[Test]
		public void ShouldNotManageTransactionWhenStageFailedAndTransactionWasAlreadyOpen()
		{
			// arrange
			var expectedException = new ArgumentNullException();

			SetupTransaction(isOpen: true);
			_stageMock
				.Setup(x => x.Execute(It.IsAny<int>()))
				.Throws(expectedException);

			// act & assert
			Assert.That(() => _sut.Execute(5), Throws.Exception.EqualTo(expectedException));

			// assert
			_baseContextMock.Verify(x => x.BeginTransaction(), Times.Never);
			_baseContextMock.Verify(x => x.CommitTransaction(), Times.Never);
			_baseContextMock.Verify(x => x.RollbackTransaction(), Times.Never);
		}

		private void SetupTransaction(bool isOpen)
		{
			var dummyTransaction = isOpen
				? (SqlTransaction)FormatterServices.GetUninitializedObject(typeof(SqlTransaction))
				: null;

			_dbContextMock
				.Setup(x => x.GetTransaction())
				.Returns(dummyTransaction);
		}
	}
}
