using DataTransfer.Legacy.MassImport.RelEyeTelemetry;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry.MetricsEventsBuilders;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.Telemetry.APM;

namespace Relativity.MassImport.Core.Pipeline.Builders
{
	internal abstract class BasePipelineBuilder
	{
		protected IPipelineExecutor PipelineExecutor { get; }
		protected IAPM APM { get; }

		protected BasePipelineBuilder(IPipelineExecutor pipelineExecutor, IAPM apm)
		{
			PipelineExecutor = pipelineExecutor;
			APM = apm;
		}

		protected IMassImportMetricsService CreateMassImportMetrics(MassImportContext context)
		{
			// TODO REL-438046 we need to figure out a better approach for dependency injection.
			return new MassImportMetrics(context.Logger, APM);
		}

		protected IRelEyeMetricsService CreateRelEyeMetricsService()
		{
			return new RelEyeMetricsService(new ApmTelemetryPublisher(APM));
		}

		protected IEventsBuilder CreateEventsBuilder()
		{
			return new EventsBuilder();
		}
	}
}
