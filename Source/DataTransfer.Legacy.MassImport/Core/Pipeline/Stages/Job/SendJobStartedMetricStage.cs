using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Input.Interface;
using System.Linq;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry.MetricsEventsBuilders;

namespace Relativity.MassImport.Core.Pipeline.Stages.Job
{
    internal class SendJobStartedMetricStage<T> : IPipelineStage<T> where T : IImportSettingsInput<Relativity.MassImport.DTO.NativeLoadInfo>
    {
        private readonly MassImportContext _context;
        private readonly IMassImportMetricsService _metricsService;
        private readonly IRelEyeMetricsService _relEyeMetricsService;
        private readonly IEventsBuilder _eventsBuilder;
        
        public SendJobStartedMetricStage(MassImportContext context, IMassImportMetricsService metricsService, IRelEyeMetricsService relEyeMetricsService, IEventsBuilder eventsBuilder)
        {
            _context = context;
            _metricsService = metricsService;
            _relEyeMetricsService = relEyeMetricsService;
            _eventsBuilder = eventsBuilder;
        }

        public T Execute(T input)
        {
            _metricsService.SendJobStarted(input.Settings, _context.JobDetails.ImportType, _context.JobDetails.ClientSystemName);
            var @event = _eventsBuilder.BuildJobStartEvent(input.Settings, _context.JobDetails.ImportType);
            _relEyeMetricsService.PublishEvent(@event);

            foreach(var field in input.Settings?.MappedFields ?? Enumerable.Empty<FieldInfo>())
            {
                _metricsService.SendFieldDetails(correlationId: input.Settings?.RunID, field);
            }

            return input;
        }
    }
}
