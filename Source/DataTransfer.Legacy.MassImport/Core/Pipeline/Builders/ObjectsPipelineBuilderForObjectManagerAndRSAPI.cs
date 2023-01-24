using System;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry.MetricsEventsBuilders;
using Relativity.Core.Service;
using Relativity.Data.MassImport;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Framework.Stages;
using Relativity.MassImport.Core.Pipeline.Input;
using Relativity.MassImport.Core.Pipeline.Stages.Job;
using Relativity.MassImport.Core.Pipeline.Stages.NotImportApi;
using Relativity.MassImport.Core.Pipeline.Stages.Shared;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.SqlFramework;
using Relativity.MassImport.Data.StagingTables;
using Relativity.Telemetry.APM;

namespace Relativity.MassImport.Core.Pipeline.Builders
{
	internal class ObjectsPipelineBuilderForObjectManagerAndRSAPI : BasePipelineBuilder
	{
		public ObjectsPipelineBuilderForObjectManagerAndRSAPI(IPipelineExecutor pipelineExecutor, IAPM apm) : base(pipelineExecutor, apm) { }

		public IPipelineStage<ObjectImportInput, MassImportManagerBase.MassImportResults> BuildPipeline(MassImportContext context, Action<TableNames> loadStagingTables)
		{
			IStagingTableRepository stagingTableRepository = new ObjectsStagingTableRepository(context.BaseContext.DBContext, context.JobDetails.TableNames, context.ImportMeasurements);
			IMassImportMetricsService metricsService = CreateMassImportMetrics(context);
			IRelEyeMetricsService relEyeMetricsService = CreateRelEyeMetricsService();
			IEventsBuilder eventsBuilder = CreateEventsBuilder();

			IPipelineStage<ObjectImportInput, MassImportManagerBase.MassImportResults> importStage = new Stages.Objects.ImportObjectsStage(context, new LockHelper(new AppLockProvider()));
			importStage = ExecuteInTransactionDecoratorStage.New(importStage, PipelineExecutor, context);
			importStage = RetryOnExceptionDecoratorStage.New(importStage, PipelineExecutor, context, actionName: "importing Objects");

			var pipeline = new SendJobStartedMetricStage<ObjectImportInput>(context, metricsService, relEyeMetricsService, eventsBuilder)
				.AddNextStage(new PopulateCacheStage<ObjectImportInput>(context), PipelineExecutor)
				.AddNextStage(new LoadColumnDefinitionCacheStage<ObjectImportInput>(context), PipelineExecutor)
				.AddNextStage(new CreateStagingTablesStage<ObjectImportInput>(stagingTableRepository), PipelineExecutor)
				.AddNextStage(new TruncateStagingTablesStage<ObjectImportInput>(stagingTableRepository), PipelineExecutor)
				.AddNextStage(new PopulateStagingTablesUsingActionStage<ObjectImportInput>(context, loadStagingTables), PipelineExecutor)
				.AddNextStage(new SendMetricWithPreImportStagingTablesDetails<ObjectImportInput>(context, stagingTableRepository, metricsService), PipelineExecutor)
				.AddNextStage(importStage, PipelineExecutor);

			return pipeline;
		}
	}
}