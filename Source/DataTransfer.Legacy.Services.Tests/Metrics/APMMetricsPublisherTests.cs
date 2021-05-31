using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using Relativity.Telemetry.APM;
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

			await _uut.Publish(metrics);

			_apmMock.Verify(x => x.CountOperation("DataTransfer.Legacy.KeplerCall", default(Guid), "", "operation(s)", true, null, metrics, null), Times.Once);
		}
	}
}
