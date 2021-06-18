using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Metrics
{
	[TestFixture]
	public class MetricsContextTests
	{
		private Mock<IMetricsPublisher> _metricsPublisher1Mock;
		private Mock<IMetricsPublisher> _metricsPublisher2Mock;
		private IMetricsContext _uut;

		[SetUp]
		public void SetUp()
		{
			_metricsPublisher1Mock = new Mock<IMetricsPublisher>();
			_metricsPublisher2Mock = new Mock<IMetricsPublisher>();
			var metricsPublishers = new List<IMetricsPublisher>
			{
				_metricsPublisher1Mock.Object,
				_metricsPublisher2Mock.Object
			};
			_uut = new MetricsContext(metricsPublishers);
		}

		[Test]
		public async Task ShouldPublishEmptyMetricsToAllPublishers()
		{
			await _uut.Publish();

			_metricsPublisher1Mock.Verify(x => x.Publish(It.Is<Dictionary<string, object>>(y => y.Count == 0)), Times.Once);
			_metricsPublisher2Mock.Verify(x => x.Publish(It.Is<Dictionary<string, object>>(y => y.Count == 0)), Times.Once);
		}

		[Test]
		public async Task ShouldPublishMetricsWithOnePropertyToAllPublishers()
		{
			var propertyName = Any.String();
			var propertyValue = Any.Object();
			_uut.PushProperty(propertyName, propertyValue);

			await _uut.Publish();

			_metricsPublisher1Mock.Verify(x => x.Publish(It.Is<Dictionary<string, object>>(y => y.Count == 1 && y[propertyName] == propertyValue)), Times.Once);
			_metricsPublisher2Mock.Verify(x => x.Publish(It.Is<Dictionary<string, object>>(y => y.Count == 1 && y[propertyName] == propertyValue)), Times.Once);
		}

		[Test]
		public async Task ShouldPublishMetricsWithTwoUniquePropertiesToAllPublishers()
		{
			var propertyName1 = Any.String();
			var propertyValue1 = Any.Object();
			var propertyName2 = Any.OtherThan(propertyName1);
			var propertyValue2 = Any.OtherThan(propertyValue1);
			_uut.PushProperty(propertyName1, propertyValue1);
			_uut.PushProperty(propertyName2, propertyValue2);

			await _uut.Publish();

			_metricsPublisher1Mock.Verify(x => x.Publish(It.Is<Dictionary<string, object>>(y => y.Count == 2
				&& y[propertyName1] == propertyValue1
				&& y[propertyName2] == propertyValue2)), Times.Once);
			_metricsPublisher2Mock.Verify(x => x.Publish(It.Is<Dictionary<string, object>>(y => y.Count == 2
				&& y[propertyName1] == propertyValue1
				&& y[propertyName2] == propertyValue2)), Times.Once);
		}

		[Test]
		public async Task ShouldPublishMetricsWithIncrementedPropertyValueAndCountPropertyToAllPublishersWhenPropertyIsTheSamePropertyPushedTwice()
		{
			var propertyName = Any.String();
			var propertyValue1 = Any.LongInteger();
			var propertyValue2 = Any.LongInteger();
			_uut.PushProperty(propertyName, propertyValue1);
			_uut.PushProperty(propertyName, propertyValue2);

			await _uut.Publish();

			_metricsPublisher1Mock.Verify(x => x.Publish(It.Is<Dictionary<string, object>>(y => y.Count == 2
				&& (long)y[propertyName] == propertyValue1 + propertyValue2
				&& (long)y[$"{propertyName}:CallsCount"] == 2L)), Times.Once);
			_metricsPublisher2Mock.Verify(x => x.Publish(It.Is<Dictionary<string, object>>(y => y.Count == 2
				&& (long)y[propertyName] == propertyValue1 + propertyValue2
				&& (long)y[$"{propertyName}:CallsCount"] == 2L)), Times.Once);
		}

		[Test]
		public async Task ShouldPublishMetricsWithIncrementedPropertyValueAndCountPropertyToAllPublishersWhenPropertyIsTheSamePropertyPushedThreeTimes()
		{
			var propertyName = Any.String();
			var propertyValue1 = Any.LongInteger();
			var propertyValue2 = Any.LongInteger();
			var propertyValue3 = Any.LongInteger();
			_uut.PushProperty(propertyName, propertyValue1);
			_uut.PushProperty(propertyName, propertyValue2);
			_uut.PushProperty(propertyName, propertyValue3);

			await _uut.Publish();

			_metricsPublisher1Mock.Verify(x => x.Publish(It.Is<Dictionary<string, object>>(y => y.Count == 2
				&& (long)y[propertyName] == propertyValue1 + propertyValue2 + propertyValue3
				&& (long)y[$"{propertyName}:CallsCount"] == 3L)), Times.Once);
			_metricsPublisher2Mock.Verify(x => x.Publish(It.Is<Dictionary<string, object>>(y => y.Count == 2
				&& (long)y[propertyName] == propertyValue1 + propertyValue2 + propertyValue3
				&& (long)y[$"{propertyName}:CallsCount"] == 3L)), Times.Once);
		}
	}
}
