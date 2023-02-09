using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.MassImport.Core;
using Relativity.MassImport.Core.Pipeline;
using Relativity.MassImport.Core.Pipeline.Input.Interface;
using Relativity.MassImport.Core.Pipeline.Stages.Job;
using System;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry.Events;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry.MetricsEventsBuilders;
using NativeLoadInfo = Relativity.MassImport.DTO.NativeLoadInfo;
using Relativity.API;

namespace DataTransfer.Legacy.MassImport.NUnit.Core.Pipeline.Stages.Job
{
	[TestFixture]
    public class SendJobStartedMetricStageTests
    {
        const string importType = "TestImport";
        const string clientName = "test42";

        private Mock<IMassImportMetricsService> _metricsServiceMock;
        private Mock<IRelEyeMetricsService> _relEyeMetricsServiceMock;
        private Mock<IEventsBuilder> _eventsBuilderMock;
        private Mock<IImportSettingsInput<NativeLoadInfo>> _inputMock;
        private NativeLoadInfo _settings;
        private SendJobStartedMetricStage<IImportSettingsInput<NativeLoadInfo>> _sut;

        [SetUp]
        public void SetUp()
        {
            var jobDetails = new MassImportJobDetails(tableNames: null, clientName, importType);
            var context = new MassImportContext(baseContext: null, loggingContext: null, jobDetails, caseSystemArtifactId: 0, new Mock<IHelper>().Object);
            _settings = new NativeLoadInfo();
            _inputMock = new Mock<IImportSettingsInput<NativeLoadInfo>>();
            _inputMock
                .Setup(x => x.Settings)
                .Returns(() => _settings);
            _metricsServiceMock = new Mock<IMassImportMetricsService>();
            _relEyeMetricsServiceMock = new Mock<IRelEyeMetricsService>();
            _eventsBuilderMock = new Mock<IEventsBuilder>();

            _sut = new SendJobStartedMetricStage<IImportSettingsInput<NativeLoadInfo>>(context, _metricsServiceMock.Object, _relEyeMetricsServiceMock.Object, _eventsBuilderMock.Object);
        }


        [Test]
        public void Execute_SendsJobStartedMetric()
        {
            // act
            _sut.Execute(_inputMock.Object);

            // assert
            _metricsServiceMock.Verify(x => x.SendJobStarted(_settings, importType, clientName));
            _eventsBuilderMock.Verify(x => x.BuildJobStartEvent(_settings, importType));
            _relEyeMetricsServiceMock.Verify(x => x.PublishEvent(It.IsAny<EventBase>()));
        }

        [Test]
        public void Execute_SendFieldDetailsMetrics_ForEachField()
        {
            // arrange
            var field1 = new FieldInfo();
            var field2 = new FieldInfo();
            _settings.MappedFields = new[] { field1, field2 };
            var runId = Guid.NewGuid().ToString();
            _settings.RunID = runId;

            // act
            _sut.Execute(_inputMock.Object);

            // assert
            _metricsServiceMock.Verify(x => x.SendFieldDetails(runId, field1), Times.Once);
            _metricsServiceMock.Verify(x => x.SendFieldDetails(runId, field2), Times.Once);
        }

        [Test]
        public void Execute_DoesNotSendFieldDetailsMetrics_WhenNoMappedFields()
        {
            // arrange
            _settings.MappedFields = new FieldInfo[0];

            // act
            _sut.Execute(_inputMock.Object);

            // assert
            _metricsServiceMock.Verify(x => x.SendFieldDetails(It.IsAny<string>(), It.IsAny<FieldInfo>()), Times.Never);
        }

        [Test]
        public void Execute_DoesNotSendFieldDetailsMetrics_WhenMappedFieldsAreNull()
        {
            // arrange
            _settings.MappedFields = null;

            // act
            _sut.Execute(_inputMock.Object);

            // assert
            _metricsServiceMock.Verify(x => x.SendFieldDetails(It.IsAny<string>(), It.IsAny<FieldInfo>()), Times.Never);
        }
    }
}
