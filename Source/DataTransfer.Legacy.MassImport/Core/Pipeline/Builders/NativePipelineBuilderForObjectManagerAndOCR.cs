using System;
using Relativity.Core.Service;
using Relativity.Data.MassImport;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Framework.Stages;
using Relativity.MassImport.Core.Pipeline.Input;
using Relativity.MassImport.Core.Pipeline.Stages.Job;
using Relativity.MassImport.Core.Pipeline.Stages.NotImportApi;
using Relativity.MassImport.Core.Pipeline.Stages.Shared;
using Relativity.MassImport.Data.StagingTables;
using Relativity.Telemetry.APM;

namespace Relativity.MassImport.Core.Pipeline.Builders
{
	internal class NativePipelineBuilderForObjectManager : BasePipelineBuilder
	{
		public NativePipelineBuilderForObjectManager(IPipelineExecutor pipelineExecutor, IAPM apm) : base(pipelineExecutor, apm) { }

		public IPipelineStage<NativeImportInput, IMassImportManagerInternal.MassImportResults> BuildPipeline(MassImportContext context, Action<TableNames> populateStagingTablesAction)
		{
			IStagingTableRepository stagingTableRepository = new NativeStagingTableRepository(context.BaseContext.DBContext, context.JobDetails.TableNames, context.ImportMeasurements);
			IMassImportMetricsService metricsService = CreateMassImportMetrics(context);

			IPipelineStage<NativeImportInput, IMassImportManagerInternal.MassImportResults> importStage = new Stages.Natives.ImportNativesStage(context);
			importStage = ExecuteInTransactionDecoratorStage.New(importStage, PipelineExecutor, context);
			importStage = RetryOnExceptionDecoratorStage.New(importStage, PipelineExecutor, context, actionName: "importing natives");

			var pipeline = new SendJobStartedMetricStage<NativeImportInput>(context, metricsService)
				.AddNextStage(new PopulateCacheStage<NativeImportInput>(context), PipelineExecutor)
				.AddNextStage(new LoadColumnDefinitionCacheStage<NativeImportInput>(context), PipelineExecutor)
				.AddNextStage(new CreateStagingTablesStage<NativeImportInput>(stagingTableRepository), PipelineExecutor)
				.AddNextStage(new TruncateStagingTablesStage<NativeImportInput>(stagingTableRepository), PipelineExecutor)
				.AddNextStage(new PopulateStagingTablesUsingActionStage<NativeImportInput>(context, populateStagingTablesAction), PipelineExecutor)
				.AddNextStage(new SendMetricWithPreImportStagingTablesDetails<NativeImportInput>(context, stagingTableRepository, metricsService), PipelineExecutor)
				.AddNextStage(new CopyExtratedTextFilesToDataGridStage(context), PipelineExecutor)
				.AddNextStage(importStage, PipelineExecutor);

			return pipeline;
		}
	}
}