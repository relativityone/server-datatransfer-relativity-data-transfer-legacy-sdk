using System;
using Moq;
using NUnit.Framework;
using Relativity.Logging;
using Relativity.MassImport.Core.Pipeline;

namespace Relativity.MassImport.NUnit.Core.Pipeline
{
	[TestFixture]
	public class LoggingContextTests
	{
		private Mock<ILog> _logMock;

		[SetUp]
		public void SetUp()
		{
			_logMock = new Mock<ILog>();
			SetupLoggerForContext(_logMock, _logMock.Object);
		}

		[Test]
		public void ShouldSetContext()
		{
			// Arrange
			string expectedCorrelationId = "123";
			string expectedClientName = "MassImportClient";

			// act
			var sut = new LoggingContext(expectedCorrelationId, expectedClientName, _logMock.Object);

			// Assert
			_logMock.Verify(x => x.ForContext("MassImport.CorrelationId", expectedCorrelationId, true));
			_logMock.Verify(x => x.ForContext("MassImport.ClientName", expectedClientName, true));
		}

		[TestCase("")]
		[TestCase("   ")]
		[TestCase(null)]
		public void ShouldGenerateCorrelationIdWhenNullOrWhitespace(string correlationId)
		{
			// Arrange
			string clientName = "MassImportClient";

			// act
			var sut = new LoggingContext(correlationId, clientName, _logMock.Object);

			// Assert
			_logMock.Verify(x => x.ForContext("MassImport.CorrelationId", It.Is<string>(actualCorrelationId => !string.IsNullOrWhiteSpace(actualCorrelationId)), true));
		}

		[TestCase("")]
		[TestCase("   ")]
		[TestCase(null)]
		public void ShouldUseUnknownClientNameWhenNullOrWhitespace(string clientName)
		{
			// Arrange
			string correlationId = "123";
			string expectedClientName = "Unknown";

			// act
			var sut = new LoggingContext(correlationId, clientName, _logMock.Object);

			// Assert
			_logMock.Verify(x => x.ForContext("MassImport.ClientName", expectedClientName, true));
		}

		[TestCase]
		public void ShouldReturnLoggerWithContext()
		{
			// Arrange
			var logWithContextMock = new Mock<ILog>();
			SetupLoggerForContext(logWithContextMock, logWithContextMock.Object);
			SetupLoggerForContext(_logMock, logWithContextMock.Object);
			var sut = new LoggingContext(correlationId: string.Empty, clientName: string.Empty, logger: _logMock.Object);

			// Act
			var actualLogger = sut.Logger;

			// Assert
			Assert.That(actualLogger, Is.EqualTo(logWithContextMock.Object), "It should return logger with context");
		}

		[Test]
		public void ShouldUseStaticLoggerWhenNoInstanceProvided()
		{
			// Arrange
			var sourceLogger = Log.Logger;
			Log.Logger = _logMock.Object;

			// Act
			var sut = new LoggingContext(correlationId: string.Empty, clientName: String.Empty);

			// Assert
			_logMock.Verify(x => x.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()));
			Log.Logger = sourceLogger;
		}

		private static void SetupLoggerForContext(Mock<ILog> loggerMockToSetup, ILog loggerToReturn)
		{
			loggerMockToSetup
				.Setup(x => x.ForContext(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<bool>()))
				.Returns(loggerToReturn);
		}
	}
}
