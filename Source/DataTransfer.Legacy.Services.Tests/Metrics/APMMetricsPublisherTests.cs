using Moq;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using Relativity.Telemetry.APM;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Metrics
{
	[TestFixture]
	public class APMMetricsPublisherTests
	{
		private Mock<IAPM> _apmMock;
		private IMetricsPublisher _uut;

		[SetUp]
		public void SetUp()
		{
			_apmMock = new Mock<IAPM>();
			_uut = new APMMetricsPublisher(_apmMock.Object);
		}

		[Test]
		public async Task ShouldPublishMetricsToAPMWhenExecuted()
		{
			var metrics = Any.Dictionary<string, object>();
			var counterOperation = new Mock<ICounterMeasure>();
			_apmMock.Setup(x => x.CountOperation(It.IsAny<string>(),
					It.IsAny<Guid>(), It.IsAny<string>(), 
					It.IsAny<string>(), It.IsAny<bool>(), 
					It.IsAny<int?>(), It.IsAny<Dictionary<string, object>>(),
					It.IsAny<IEnumerable<ISink>>()))
				.Returns(counterOperation.Object);

			await _uut.Publish(metrics);

			_apmMock.Verify(x => x.CountOperation("DataTransfer.Legacy.KeplerCall", default(Guid), "", "operation(s)", true, null, metrics, null), Times.Once);
			counterOperation.Verify(m=>m.Write(), Times.Once);
		}

		[Test]
		public async Task ShouldPublishHealthCheckResultWhenExecuted([Values(true, false)] bool isHealthy)
		{
			//Arrange
			var healthMeasureMock = new Mock<IHealthMeasure>();
			_apmMock.Setup(x => x.HealthCheckOperation(It.IsAny<string>(),
					It.IsAny<Func<HealthCheckOperationResult>>(),
					It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(),
					It.IsAny<IEnumerable<ISink>>()))
				.Returns(healthMeasureMock.Object);
			
			//Act
			await _uut.PublishHealthCheckResult(isHealthy, "Message");

			//Assert
			_apmMock.Verify(x => x.HealthCheckOperation("DataTransfer.Legacy.HealthCheckMetric",
				It.Is<Func<HealthCheckOperationResult>>(f => f.Invoke().IsHealthy == isHealthy && f.Invoke().Message == "Message"),
				It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(),
				It.IsAny<IEnumerable<ISink>>()));
		}
		[Test]
		public async Task ShouldCallWriteHealthCheckResultWhenExecuted([Values(true, false)] bool isHealthy)
		{
			//Arrange
			var healthMeasureMock = new Mock<IHealthMeasure>();
			_apmMock.Setup(x => x.HealthCheckOperation(It.IsAny<string>(),
					It.IsAny<Func<HealthCheckOperationResult>>(),
					It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(),
					It.IsAny<IEnumerable<ISink>>()))
				.Returns(healthMeasureMock.Object);
			
			//Act
			await _uut.PublishHealthCheckResult(isHealthy, "Message");

			//Assert
			healthMeasureMock.Verify(x => x.Write(), Times.Once);
		}
	}
}