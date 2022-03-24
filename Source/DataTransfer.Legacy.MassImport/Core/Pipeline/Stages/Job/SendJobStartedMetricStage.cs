using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Input.Interface;

namespace Relativity.MassImport.Core.Pipeline.Stages.Job
{
	internal class SendJobStartedMetricStage<T> : IPipelineStage<T> where T : IImportSettingsInput<Relativity.MassImport.DTO.NativeLoadInfo>
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
			return input;
		}
	}
}
