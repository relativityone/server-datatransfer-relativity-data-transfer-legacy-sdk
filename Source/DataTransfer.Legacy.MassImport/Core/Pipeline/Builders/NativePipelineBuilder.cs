using DataTransfer.Legacy.MassImport.RelEyeTelemetry;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry.MetricsEventsBuilders;
using Relativity.Core.Service;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Framework.Stages;
using Relativity.MassImport.Core.Pipeline.Input;
using Relativity.MassImport.Core.Pipeline.Stages.Job;
using Relativity.MassImport.Core.Pipeline.Stages.Natives;
using Relativity.MassImport.Core.Pipeline.Stages.Shared;
using Relativity.MassImport.Data.StagingTables;
using Relativity.Telemetry.APM;

namespace Relativity.MassImport.Core.Pipeline.Builders
{
	internal class NativePipelineBuilder : BasePipelineBuilder
	{
		public NativePipelineBuilder(IPipelineExecutor pipelineExecutor, IAPM apm) : base(pipelineExecutor, apm) { }

		public IPipelineStage<NativeImportInput, MassImportManagerBase.MassImportResults> BuildPipeline(MassImportContext context)
		{
			IStagingTableRepository stagingTableRepository = new NativeStagingTableRepository(context.BaseContext.DBContext, context.JobDetails.TableNames, context.ImportMeasurements);
			IMassImportMetricsService metricsService = CreateMassImportMetrics(context);
			IRelEyeMetricsService relEyeMetricsService = CreateRelEyeMetricsService();
			IEventsBuilder eventsBuilder = CreateEventsBuilder();

			var pipeline = BuildJobInitializationStage(context, stagingTableRepository, metricsService, relEyeMetricsService, eventsBuilder)
				.AddNextStage(BuildBatchExecutionStage(context, stagingTableRepository, metricsService, relEyeMetricsService, eventsBuilder), PipelineExecutor);

			return pipeline;
		}

		private IPipelineStage<NativeImportInput> BuildJobInitializationStage(MassImportContext context, IStagingTableRepository stagingTableRepository, IMassImportMetricsService metricsService, IRelEyeMetricsService relEyeMetricsService, IEventsBuilder eventsBuilder)
		{
			var jobStage = new SendJobStartedMetricStage<NativeImportInput>(context, metricsService, relEyeMetricsService, eventsBuilder)
				.AddNextStage(new PopulateCacheStage<NativeImportInput>(context), PipelineExecutor)
				.AddNextStage(new LoadColumnDefinitionCacheStage<NativeImportInput>(context), PipelineExecutor)
				.AddNextStage(new CreateStagingTablesStage<NativeImportInput>(stagingTableRepository), PipelineExecutor);

			return new ExecuteIfJobNotInitializedStage<NativeImportInput>(PipelineExecutor, jobStage, stagingTableRepository);
		}

		private IPipelineStage<NativeImportInput, MassImportManagerBase.MassImportResults> BuildBatchExecutionStage(
			MassImportContext context,
			IStagingTableRepository stagingTableRepository,
			IMassImportMetricsService metricsService,
			IRelEyeMetricsService relEyeMetricsService,
			IEventsBuilder eventsBuilder)
		{
			var batchStage = BuildPreImportStage(context, stagingTableRepository, metricsService)
				.AddNextStage(BuildImportStage(context), PipelineExecutor);

			return batchStage;
		}

		private IPipelineStage<NativeImportInput, NativeImportInput> BuildPreImportStage(MassImportContext context, IStagingTableRepository stagingTableRepository, IMassImportMetricsService metricsService)
		{
			IPipelineStage<NativeImportInput, NativeImportInput> createFoldersStage = new CreateFoldersStage<NativeImportInput>(context);
			createFoldersStage = ExecuteInTransactionDecoratorStage.New(createFoldersStage, PipelineExecutor, context);
			createFoldersStage = RetryOnExceptionDecoratorStage.New(createFoldersStage, PipelineExecutor, context, "creating folders");

			IPipelineStage<NativeImportInput, NativeImportInput> importMetadataFilesToStagingTablesStage = new ImportMetadataFilesToStagingTablesStage<NativeImportInput>(context, stagingTableRepository);
			importMetadataFilesToStagingTablesStage = RetryOnExceptionDecoratorStage.New(importMetadataFilesToStagingTablesStage, PipelineExecutor, context, "bulk insert temp files");

			return new ValidateSettingsStage<NativeImportInput>()
				.AddNextStage(new TruncateStagingTablesStage<NativeImportInput>(stagingTableRepository), PipelineExecutor)
				.AddNextStage(new LoadColumnDefinitionCacheStage<NativeImportInput>(context), PipelineExecutor)
				.AddNextStage(importMetadataFilesToStagingTablesStage, PipelineExecutor)
				.AddNextStage(new SendMetricWithPreImportStagingTablesDetails<NativeImportInput>(context, stagingTableRepository, metricsService), PipelineExecutor)
				.AddNextStage(createFoldersStage, PipelineExecutor)
				.AddNextStage(new CopyFullTextFromFileShareLocationStage(context), PipelineExecutor)
				.AddNextStage(new CopyExtratedTextFilesToDataGridStage(context), PipelineExecutor);
		}

		private IPipelineStage<NativeImportInput, MassImportManagerBase.MassImportResults> BuildImportStage(MassImportContext context)
		{
			IPipelineStage<NativeImportInput, MassImportManagerBase.MassImportResults> importStage = new ImportNativesStage(context);
			importStage = ExecuteInTransactionDecoratorStage.New(importStage, PipelineExecutor, context);
			importStage = RetryOnExceptionDecoratorStage.New(importStage, PipelineExecutor, context, actionName: "importing natives");
			importStage = new SendLegacyImportMetricDecoratorStage<NativeImportInput>(
				importStage,
				PipelineExecutor,
				context,
				metricName: "Document",
				APM);
			return importStage;
		}
	}
}