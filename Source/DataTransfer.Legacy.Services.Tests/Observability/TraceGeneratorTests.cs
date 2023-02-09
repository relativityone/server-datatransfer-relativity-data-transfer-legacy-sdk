using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.Observability;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TelemetryConstants = DataTransfer.Legacy.MassImport.RelEyeTelemetry.TelemetryConstants;

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

			Assert.That(activity.GetTagItem(TelemetryConstants.AttributeNames.R1TeamID), Is.EqualTo(TelemetryConstants.Values.R1TeamID));
			Assert.That(activity.GetTagItem(TelemetryConstants.AttributeNames.ServiceNamespace), Is.EqualTo(TelemetryConstants.Values.ServiceNamespace));
			Assert.That(activity.GetTagItem(TelemetryConstants.AttributeNames.ServiceName), Is.EqualTo(TelemetryConstants.Values.ServiceName));
			Assert.That(activity.GetTagItem(TelemetryConstants.AttributeNames.ApplicationID), Is.EqualTo(TelemetryConstants.Values.ApplicationID));
			Assert.That(activity.GetTagItem(TelemetryConstants.AttributeNames.ApplicationName), Is.EqualTo(TelemetryConstants.Values.ApplicationName));

			_instanceSettingsBundleMock.Verify(m => m.GetStringAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
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

			Assert.That(activity.GetTagItem(TelemetryConstants.AttributeNames.R1TeamID), Is.EqualTo(TelemetryConstants.Values.R1TeamID));
			Assert.That(activity.GetTagItem(TelemetryConstants.AttributeNames.ServiceNamespace), Is.EqualTo(TelemetryConstants.Values.ServiceNamespace));
			Assert.That(activity.GetTagItem(TelemetryConstants.AttributeNames.ServiceName), Is.EqualTo(TelemetryConstants.Values.ServiceName));
			Assert.That(activity.GetTagItem(TelemetryConstants.AttributeNames.ApplicationID), Is.EqualTo(TelemetryConstants.Values.ApplicationID));
			Assert.That(activity.GetTagItem(TelemetryConstants.AttributeNames.ApplicationName), Is.EqualTo(TelemetryConstants.Values.ApplicationName));

			_instanceSettingsBundleMock.Verify(m => m.GetStringAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
			_loggerMock.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
		}
	}
}
