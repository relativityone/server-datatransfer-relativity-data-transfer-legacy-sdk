using System.Linq;
using Moq;
using NUnit.Framework;
using Relativity.Logging;
using Relativity.MassImport.Core.Pipeline;
using Relativity.MassImport.Core.Pipeline.Errors;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Stages.Shared;

namespace Relativity.MassImport.NUnit.Core.Pipeline.Stages.Shared
{
	[TestFixture]
	public class RetryOnExceptionDecoratorStageTests
	{
		private const string ActionName = "RetryTests";
		private const int NumberOfRetries = 3;

		private Mock<IPipelineStage<int>> _stageMock;
		private Mock<ILog> _loggerMock;
		private PipelineExecutor _pipelineExecutor;
		private MassImportContext _massImportContext;
		private RetryOnExceptionDecoratorStage<int, int> _sut;

		[SetUp]
		public void SetUp()
		{
			_pipelineExecutor = new PipelineExecutor();
			_stageMock = new Mock<IPipelineStage<int>>();
			_loggerMock = new Mock<ILog>();
			_loggerMock
				.Setup(x => x.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
				.Returns(_loggerMock.Object);

			_massImportContext = new MassImportContext(
				baseContext: null, // not used by ExecuteInTransactionDecoratorStage
				new LoggingContext("correlationId", "clientName", _loggerMock.Object),
				jobDetails: null, // not used by ExecuteInTransactionDecoratorStage
				caseSystemArtifactId: -1); // not used by ExecuteInTransactionDecoratorStage

			_sut = new RetryOnExceptionDecoratorStage<int, int>(
				_stageMock.Object,
				_pipelineExecutor,
				_massImportContext,
				ActionName,
				exponentialWaitTimeBase: 0,
				NumberOfRetries);
		}

		[Test]
		public void ShouldReturnResultWhenFirstCallSucceeded()
		{
			// arrange
			const int expectedResult = 3;
			_stageMock
				.Setup(x => x.Execute(It.IsAny<int>()))
				.Returns(expectedResult);

			// act
			int actualResult = _sut.Execute(5);

			// assert
			Assert.That(_massImportContext.ImportMeasurements.GetCounters(), Is.Empty, "It should not retry when first call succeeded.");

			Assert.That(actualResult, Is.EqualTo(expectedResult));
		}

		[TestCase(MassImportErrorCategory.SqlCategory)]
		[TestCase(MassImportErrorCategory.UnknownCategory)]
		public void ShouldThrowExceptionWithoutRetryWhenErrorIsNonRetryable(string errorCategory)
		{
			// arrange
			var expectedException = new MassImportExecutionException("message", "stage", errorCategory, inner: null);
			_stageMock
				.Setup(x => x.Execute(It.IsAny<int>()))
				.Throws(expectedException);

			// act & assert
			Assert.That(() => _sut.Execute(5), Throws.Exception.EqualTo(expectedException));

			// assert
			Assert.That(_massImportContext.ImportMeasurements.GetCounters(), Is.Empty, "It should not retry when error is not retryable.");
			_loggerMock.Verify(x => x.LogError(expectedException, "Error occurred while {action}", ActionName));
		}

		[TestCase(MassImportErrorCategory.TimeoutCategory)]
		[TestCase(MassImportErrorCategory.DeadlockCategory)]
		public void ShouldRetryWhenFirstCallFailed(string errorCategory)
		{
			// arrange
			const int expectedResult = 3;
			var expectedException = new MassImportExecutionException("message", "stage", errorCategory, inner: null);
			_stageMock
				.SetupSequence(x => x.Execute(It.IsAny<int>()))
				.Throws(expectedException)
				.Returns(expectedResult);

			// act
			int actualResult = _sut.Execute(5);

			// assert
			Assert.That(actualResult, Is.EqualTo(expectedResult), "It should return result because second call succeeded.");
			var counter = _massImportContext.ImportMeasurements.GetCounters().Single();
			Assert.That(counter.Key, Is.EqualTo($"Retry-'{ActionName}'"));
			Assert.That(counter.Value, Is.EqualTo(1));

			if (errorCategory == MassImportErrorCategory.TimeoutCategory)
			{
				_loggerMock.Verify(x => x.LogError(expectedException, "Timeout occurred while {action}.", ActionName));
			}
			else if (errorCategory == MassImportErrorCategory.DeadlockCategory)
			{
				_loggerMock.Verify(x => x.LogError(expectedException, "Deadlock occurred while {action}.", ActionName));
			}
		}

		[Test]
		public void ShouldThrowWhenSecondErrorIsNonRetryable()
		{
			// arrange
			var retryableException = new MassImportExecutionException("message", "stage", MassImportErrorCategory.DeadlockCategory, inner: null);
			var nonRetryableException = new MassImportExecutionException("message", "stage", MassImportErrorCategory.SqlCategory, inner: null);
			_stageMock
				.SetupSequence(x => x.Execute(It.IsAny<int>()))
				.Throws(retryableException)
				.Throws(nonRetryableException);

			// act & assert
			Assert.That(() => _sut.Execute(5), Throws.Exception.EqualTo(nonRetryableException));

			// assert
			var counter = _massImportContext.ImportMeasurements.GetCounters().Single();
			Assert.That(counter.Key, Is.EqualTo($"Retry-'{ActionName}'"));
			Assert.That(counter.Value, Is.EqualTo(1));

			_loggerMock.Verify(x => x.LogError(retryableException, "Deadlock occurred while {action}.", ActionName));
			_loggerMock.Verify(x => x.LogError(nonRetryableException, "Error occurred while {action}", ActionName));
		}

		[Test]
		public void ShouldThrowWhenAllRetriesFailed()
		{
			// arrange
			var expectedException = new MassImportExecutionException("message", "stage", MassImportErrorCategory.DeadlockCategory, inner: null);
			_stageMock
				.Setup(x => x.Execute(It.IsAny<int>()))
				.Throws(expectedException);

			// act
			Assert.That(() => _sut.Execute(5), Throws.Exception.EqualTo(expectedException));

			// assert
			var counter = _massImportContext.ImportMeasurements.GetCounters().Single();
			Assert.That(counter.Key, Is.EqualTo($"Retry-'{ActionName}'"));
			Assert.That(counter.Value, Is.EqualTo(NumberOfRetries));

			_loggerMock.Verify(x => x.LogError(expectedException, "Deadlock occurred while {action}.", ActionName), Times.Exactly(NumberOfRetries));
		}

		/// <summary>
		/// 2 is a default exponential base, so for 8 retries, it will wait for 510 seconds in total.
		/// </summary>
		[Test]
		public void ShouldDoNotRetryMoreThanEightTimes()
		{
			// arrange
			const int requestedNumberOfRetries = 9;
			const int expectedNumberOfRetries = 8;

			_sut = new RetryOnExceptionDecoratorStage<int, int>(
				_stageMock.Object,
				_pipelineExecutor,
				_massImportContext,
				ActionName,
				exponentialWaitTimeBase: 0,
				requestedNumberOfRetries);

			var expectedException = new MassImportExecutionException("message", "stage", MassImportErrorCategory.DeadlockCategory, inner: null);
			_stageMock
				.Setup(x => x.Execute(It.IsAny<int>()))
				.Throws(expectedException);

			// act
			Assert.That(() => _sut.Execute(5), Throws.Exception);

			// assert
			var counter = _massImportContext.ImportMeasurements.GetCounters().Single();
			Assert.That(counter.Key, Is.EqualTo($"Retry-'{ActionName}'"));
			Assert.That(counter.Value, Is.EqualTo(expectedNumberOfRetries));
		}
	}
}
