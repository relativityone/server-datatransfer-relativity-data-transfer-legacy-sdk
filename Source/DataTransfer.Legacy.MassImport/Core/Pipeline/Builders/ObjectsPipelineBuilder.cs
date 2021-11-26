using Relativity.Core.Service;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Framework.Stages;
using Relativity.MassImport.Core.Pipeline.Input;
using Relativity.MassImport.Core.Pipeline.Stages.Job;
using Relativity.MassImport.Core.Pipeline.Stages.Objects;
using Relativity.MassImport.Core.Pipeline.Stages.Shared;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.SqlFramework;
using Relativity.MassImport.Data.StagingTables;
using Relativity.Telemetry.APM;

namespace Relativity.MassImport.Core.Pipeline.Builders
{
	internal class ObjectsPipelineBuilder : BasePipelineBuilder
	{
		public ObjectsPipelineBuilder(IPipelineExecutor pipelineExecutor, IAPM apm) : base(pipelineExecutor, apm) { }

		public IPipelineStage<ObjectImportInput, IMassImportManagerInternal.MassImportResults> BuildPipeline(MassImportContext context)
		{
			IStagingTableRepository stagingTableRepository = new ObjectsStagingTableRepository(context.BaseContext.DBContext, context.JobDetails.TableNames, context.ImportMeasurements);
			IMassImportMetricsService metricsService = CreateMassImportMetrics(context);

			var pipeline = BuildJobInitializationStage(context, stagingTableRepository, metricsService)
				.AddNextStage(BuildBatchExecutionStage(context, stagingTableRepository, metricsService), PipelineExecutor);

			return pipeline;
		}

		private IPipelineStage<ObjectImportInput> BuildJobInitializationStage(MassImportContext context, IStagingTableRepository stagingTableRepository, IMassImportMetricsService metricsService)
		{
			var jobStage = new SendJobStartedMetricStage<ObjectImportInput>(context, metricsService)
				.AddNextStage(new PopulateCacheStage<ObjectImportInput>(context), PipelineExecutor)
				.AddNextStage(new LoadColumnDefinitionCacheStage<ObjectImportInput>(context), PipelineExecutor)
				.AddNextStage(new CreateStagingTablesStage<ObjectImportInput>(stagingTableRepository), PipelineExecutor);

			return new ExecuteIfJobNotInitializedStage<ObjectImportInput>(PipelineExecutor, jobStage, stagingTableRepository);
		}

		private IPipelineStage<ObjectImportInput, IMassImportManagerInternal.MassImportResults> BuildBatchExecutionStage(
			MassImportContext context,
			IStagingTableRepository stagingTableRepository,
			IMassImportMetricsService metricsService)
		{
			var batchStage = BuildPreImportStage(context, stagingTableRepository, metricsService)
				.AddNextStage(BuildImportStage(context), PipelineExecutor);

			return batchStage;
		}

		private IPipelineStage<ObjectImportInput> BuildPreImportStage(MassImportContext context, IStagingTableRepository stagingTableRepository, IMassImportMetricsService metricsService)
		{
			IPipelineStage<ObjectImportInput> importMetadataStage = new ImportMetadataFilesToStagingTablesStage<ObjectImportInput>(context, stagingTableRepository);
			importMetadataStage = new ExecuteIfRangeIsNotDefinedStage<ObjectImportInput>(PipelineExecutor, importMetadataStage);

			return new ValidateSettingsStage<ObjectImportInput>()
				.AddNextStage(new TruncateStagingTablesStage<ObjectImportInput>(stagingTableRepository), PipelineExecutor)
				.AddNextStage(new LoadColumnDefinitionCacheStage<ObjectImportInput>(context), PipelineExecutor)
				.AddNextStage(importMetadataStage, PipelineExecutor)
				.AddNextStage(new SendMetricWithPreImportStagingTablesDetails<ObjectImportInput>(context, stagingTableRepository, metricsService), PipelineExecutor);
		}

		private IPipelineStage<ObjectImportInput, IMassImportManagerInternal.MassImportResults> BuildImportStage(MassImportContext context)
		{
			IPipelineStage<ObjectImportInput, IMassImportManagerInternal.MassImportResults> importStage = new ImportObjectsStage(context, new LockHelper(new AppLockProvider()));
			importStage = ExecuteInTransactionDecoratorStage.New(importStage, PipelineExecutor, context);
			importStage = RetryOnExceptionDecoratorStage.New(importStage, PipelineExecutor, context, actionName: "importing Objects");
			importStage = new SendLegacyImportMetricDecoratorStage<ObjectImportInput>(
				importStage,
				PipelineExecutor,
				context,
				metricName: "Object",
				APM);
			return importStage;
		}
	}
}