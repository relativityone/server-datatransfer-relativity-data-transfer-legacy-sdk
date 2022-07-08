using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Metrics
{
	[TestFixture]
	public class LoggingMetricsPublisherTests
	{
		private Mock<IAPILog> _loggerMock;
		private IMetricsPublisher _uut;

		[SetUp]
		public void SetUp()
		{
			_loggerMock = new Mock<IAPILog>();
			_uut = new LoggingMetricsPublisher(_loggerMock.Object);
		}

		[Test]
		public async Task ShouldLogMetricWhenPublished()
		{
			var metrics = Any.Dictionary<string, object>();

			await _uut.Publish(metrics);

			_loggerMock.Verify(x => x.LogInformation("DataTransfer.Legacy Metrics: {@metrics}", metrics), Times.Once);
		}


		[Test]
		public async Task ShouldLogHealthCheckWhenPublished([Values(true, false)] bool isHealthy)
		{
			await _uut.PublishHealthCheckResult(isHealthy, "Message");
			if (isHealthy)
			{
				_loggerMock.Verify(x => x.LogInformation("Health Check Result: {@isHealthy}, message: {@message}", isHealthy, "Message"), Times.Once);
			}
			else
			{
				_loggerMock.Verify(x => x.LogError("Health Check Result: {@isHealthy}, message: {@message}", isHealthy, "Message"), Times.Once);

			}
		}
	}
}