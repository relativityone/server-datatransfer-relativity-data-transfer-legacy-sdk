using DataTransfer.Legacy.MassImport.Core;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.Observability;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Observability
{
	[TestFixture]
	public class TraceGeneratorTests
	{
		private Mock<IAPILog> _loggerMock;
		private Mock<IInstanceSettingsBundle> _instanceSettingsBundleMock;
		private TraceGenerator _sut = null;

		[SetUp]
		public void SetUp()
		{
			_loggerMock = new Mock<IAPILog>();
			_instanceSettingsBundleMock = new Mock<IInstanceSettingsBundle>();
			_sut = new TraceGenerator(_loggerMock.Object, _instanceSettingsBundleMock.Object);
		}

		[Test]
		public void TraceGenerator_ReturnsStartedActivity_WhichHasSystemTags()
		{
			// arrange
			_instanceSettingsBundleMock.Setup(m => m.GetStringAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult("http://test.test"));

			// act
			var activity = _sut.StartActivity("test", ActivityKind.Server, default(ActivityContext));

			// assert
			Assert.That(activity, Is.Not.Null);

			Assert.That(activity.GetTagItem(TelemetryConstants.MetricsAttributes.OwnerTeamId), Is.EqualTo(TelemetryConstants.Application.OwnerTeamId));
			Assert.That(activity.GetTagItem(TelemetryConstants.MetricsAttributes.SystemName), Is.EqualTo(TelemetryConstants.Application.SystemName));
			Assert.That(activity.GetTagItem(TelemetryConstants.MetricsAttributes.ServiceName), Is.EqualTo(TelemetryConstants.Application.ServiceName));
			Assert.That(activity.GetTagItem(TelemetryConstants.MetricsAttributes.ApplicationID), Is.EqualTo(TelemetryConstants.Application.ApplicationID));
			Assert.That(activity.GetTagItem(TelemetryConstants.MetricsAttributes.ApplicationName), Is.EqualTo(TelemetryConstants.Application.ApplicationName));

			_instanceSettingsBundleMock.Verify(m => m.GetStringAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
			_loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
		}

		[Test]
		public void TraceGenerator_ReturnsNullActivity_WhenCannotReadReleyeInstanceSettings()
		{
			// arrange
			_instanceSettingsBundleMock.Setup(m => m.GetStringAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("test"));

			// act
			var activity = _sut.StartActivity("test", ActivityKind.Server, default(ActivityContext));

			// assert
			Assert.That(activity, Is.Null);

			_loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
		}

		[Test]
		public void TraceGenerator_ReturnsStartedActivity_WhichWasStartedUsingTheSameTraceProvider()
		{
			// arrange
			_instanceSettingsBundleMock.Setup(m => m.GetStringAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult("http://test.test"));

			// act
			var activity = _sut.StartActivity("test1", ActivityKind.Server, default(ActivityContext));
			activity = _sut.StartActivity("test2", ActivityKind.Server, default(ActivityContext));
			activity = _sut.StartActivity("test3", ActivityKind.Server, default(ActivityContext));
			activity = _sut.StartActivity("test4", ActivityKind.Server, default(ActivityContext));
			activity = _sut.StartActivity("test5", ActivityKind.Server, default(ActivityContext));
			activity = _sut.StartActivity("test", ActivityKind.Server, default(ActivityContext));


			// assert
			Assert.That(activity, Is.Not.Null);

			Assert.That(activity.GetTagItem(TelemetryConstants.MetricsAttributes.OwnerTeamId), Is.EqualTo(TelemetryConstants.Application.OwnerTeamId));
			Assert.That(activity.GetTagItem(TelemetryConstants.MetricsAttributes.SystemName), Is.EqualTo(TelemetryConstants.Application.SystemName));
			Assert.That(activity.GetTagItem(TelemetryConstants.MetricsAttributes.ServiceName), Is.EqualTo(TelemetryConstants.Application.ServiceName));
			Assert.That(activity.GetTagItem(TelemetryConstants.MetricsAttributes.ApplicationID), Is.EqualTo(TelemetryConstants.Application.ApplicationID));
			Assert.That(activity.GetTagItem(TelemetryConstants.MetricsAttributes.ApplicationName), Is.EqualTo(TelemetryConstants.Application.ApplicationName));

			_instanceSettingsBundleMock.Verify(m => m.GetStringAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
			_loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
		}
	}
}
