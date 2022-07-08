using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Input.Interface;
using System.Linq;

namespace Relativity.MassImport.Core.Pipeline.Stages.Job
{
    internal class SendJobStartedMetricStage<T> : IPipelineStage<T> where T : IImportSettingsInput<NativeLoadInfo>
    {
        private readonly MassImportContext _context;
        private readonly IMassImportMetricsService _metricsService;


        public SendJobStartedMetricStage(MassImportContext context, IMassImportMetricsService metricsService)
        {
            _context = context;
            _metricsService = metricsService;
        }

        public T Execute(T input)
        {
            _metricsService.SendJobStarted(input.Settings, _context.JobDetails.ImportType, _context.JobDetails.ClientSystemName);
            
            foreach(var field in input.Settings?.MappedFields ?? Enumerable.Empty<FieldInfo>())
            {
                _metricsService.SendFieldDetails(correlationId: input.Settings?.RunID, field);
            }

            return input;
        }
    }
}
