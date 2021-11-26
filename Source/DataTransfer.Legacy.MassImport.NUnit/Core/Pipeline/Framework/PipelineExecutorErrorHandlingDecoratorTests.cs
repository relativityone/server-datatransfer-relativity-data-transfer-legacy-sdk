using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Relativity.Logging;
using Relativity.MassImport.Core.Pipeline;
using Relativity.MassImport.Core.Pipeline.Errors;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Framework.Stages;

namespace Relativity.MassImport.NUnit.Core.Pipeline.Framework
{
	[TestFixture]
	public class PipelineExecutorErrorHandlingDecoratorTests
	{
		private PipelineExecutorErrorHandlingDecorator _sut;

		private Mock<IPipelineExecutor> _pipelineExecutorMock;
		private Mock<ILog> _logMock;

		[SetUp]
		public void SetUp()
		{
			_pipelineExecutorMock = new Mock<IPipelineExecutor>();
			_logMock = new Mock<ILog>(MockBehavior.Strict);
			_logMock.Setup(x => x.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
				.Returns(_logMock.Object);

			var loggingContext = new LoggingContext("1", "testClient", _logMock.Object);

			_sut = new PipelineExecutorErrorHandlingDecorator(_pipelineExecutorMock.Object, loggingContext);
		}

		[Test]
		public void ShouldNotLogForInfrastructureStageWhichSucceeded()
		{
			// Arrange
			var infrastructureStageMock = new Mock<IInfrastructureStage>();
			var stageMock = infrastructureStageMock.As<IPipelineStage<int>>();

			// Act
			try
			{
				_sut.Execute(stageMock.Object, 4);
			}
			catch (MockException) // logger mock has strict behavior
			{
				Assert.Fail("logger should not be used for infrastructure stages");
			}

			// Assert
			_pipelineExecutorMock.Verify(x => x.Execute(stageMock.Object, 4), Times.Once);
		}

		[Test]
		public void ShouldNotLogForInfrastructureStageWhichFailed()
		{
			// Arrange
			var exceptionToThrow = new System.Exception();
			int input = 4;

			var infrastructureStageMock = new Mock<IInfrastructureStage>();
			var stageMock = infrastructureStageMock.As<IPipelineStage<int>>();
			_pipelineExecutorMock
				.Setup(x => x.Execute(stageMock.Object, It.IsAny<int>()))
				.Throws(exceptionToThrow);

			// Act
			try
			{
				_sut.Execute(stageMock.Object, input);
				Assert.Fail("It should rethrow exception");
			}
			catch (MockException) // logger mock has strict behavior
			{
				Assert.Fail("logger should not be used for infrastructure stages");
			}
			catch (System.Exception actualException)
			{
				Assert.AreEqual(exceptionToThrow, actualException);
			}

			// Assert
			_pipelineExecutorMock.Verify(x => x.Execute(stageMock.Object, input), Times.Once);
		}

		[Test]
		public void ShouldLogStageWhichSucceeded()
		{
			// Arrange
			var stageMock = new Mock<IPipelineStage<int>>();
			_logMock.Setup(x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()));

			// Act
			_sut.Execute(stageMock.Object, 4);

			// Assert
			_pipelineExecutorMock.Verify(x => x.Execute(stageMock.Object, 4), Times.Once);
		}

		[Test]
		public void ShouldLogAndWrapExceptionWhenStageFailed()
		{
			// Arrange
			var originalException = new System.Exception();
			int input = 4;

			var stageMock = new Mock<IPipelineStage<int>>();
			_pipelineExecutorMock
				.Setup(x => x.Execute(stageMock.Object, It.IsAny<int>()))
				.Throws(originalException);
			_logMock.Setup(x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()));
			_logMock.Setup(x => x.LogError(originalException, It.IsAny<string>(), It.IsAny<object[]>()));

			// Act & Assert
			IResolveConstraint expectedExceptionConstraint = Throws
				.Exception.TypeOf<MassImportExecutionException>()
				.With.Message.Contain("Error occured while executing")
				.And.InnerException.EqualTo(originalException);

			var ex = expectedExceptionConstraint.ToString();
			Assert.That(() => _sut.Execute(stageMock.Object, input), expectedExceptionConstraint, "It should wrap exception");

			// Assert
			_pipelineExecutorMock.Verify(x => x.Execute(stageMock.Object, input), Times.Once);
		}

		[Test]
		public void ShouldRethrowMassImportExecutionException()
		{
			// Arrange
			var expectedException = new MassImportExecutionException();
			int input = 4;

			var stageMock = new Mock<IPipelineStage<int>>();
			_pipelineExecutorMock
				.Setup(x => x.Execute(stageMock.Object, It.IsAny<int>()))
				.Throws(expectedException);
			_logMock.Setup(x => x.LogDebug(It.IsAny<string>(), It.IsAny<object[]>()));
			_logMock.Setup(x => x.LogError(expectedException, It.IsAny<string>(), It.IsAny<object[]>()));

			// Act & Assert
			Assert.That(() => _sut.Execute(stageMock.Object, input), Throws.Exception.EqualTo(expectedException), "It should rethrow exception when it was already handled");

			// Assert
			_pipelineExecutorMock.Verify(x => x.Execute(stageMock.Object, input), Times.Once);
		}
	}
}
