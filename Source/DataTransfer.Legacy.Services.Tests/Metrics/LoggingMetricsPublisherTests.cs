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

			_loggerMock.Verify(x => x.LogInformation("Metrics: {@metrics}", metrics), Times.Once);
		}
	}
}
