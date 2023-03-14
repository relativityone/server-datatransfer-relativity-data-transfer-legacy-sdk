using System;
using System.Diagnostics;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry.MetricsEventsBuilders;
using Relativity.Data.MassImport;
using Relativity.MassImport.Core;
using Relativity.MassImport.Core.Pipeline;
using Relativity.MassImport.Core.Pipeline.Builders;
using Relativity.MassImport.Core.Pipeline.Errors;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Input;
using Relativity.Telemetry.APM;
using Relativity.MassImport.Data;
using Relativity.MassImport.Data.DataGrid;
using Relativity.API;

namespace Relativity.Core.Service.MassImport
{
	internal class MassImporter
	{
		private static IAPM APMClient => Client.APMClient;

		internal static MassImportManagerBase.MassImportResults ImportNatives(BaseContext baseContext, NativeImportInput input, IHelper helper)
		{
			var contextAndExecutorDto = CreateMassImportContextAndPipelineExecutor(
				baseContext,
				input.Settings,
				Relativity.MassImport.Core.Constants.SystemNames.Kepler,
				Relativity.MassImport.Core.Constants.ImportType.Natives,
				helper);

			var pipelineBuilder = new NativePipelineBuilder(contextAndExecutorDto.PipelineExecutor, APMClient);
			var pipeline = pipelineBuilder.BuildPipeline(contextAndExecutorDto.MassImportContext);
			var result = ExecuteImport(pipeline, input, input.Settings, contextAndExecutorDto);
			return result;
		}

		internal static MassImportManagerBase.MassImportResults ImportObjects(BaseContext baseContext, ObjectImportInput input, IHelper helper)
		{
			var contextAndExecutorDto = CreateMassImportContextAndPipelineExecutor(
				baseContext,
				input.Settings,
				Relativity.MassImport.Core.Constants.SystemNames.Kepler,
				Relativity.MassImport.Core.Constants.ImportType.Objects,
				helper);

			var pipelineBuilder = new ObjectsPipelineBuilder(contextAndExecutorDto.PipelineExecutor, APMClient);
			var pipeline = pipelineBuilder.BuildPipeline(contextAndExecutorDto.MassImportContext);
			var result = ExecuteImport(pipeline, input, input.Settings, contextAndExecutorDto);
			return result;
		}

		public static MassImportManagerBase.MassImportResults ImportNativesForObjectManager(BaseContext baseContext, Relativity.MassImport.DTO.NativeLoadInfo settings, Action<TableNames> loadStagingTablesAction, DataGridReader dataGridReader, IHelper helper)
		{
			IDataGridInputReaderProvider dataGridInputReaderProvider = dataGridReader is null ? null : new DataGridInputReaderProvider(dataGridReader);
			var input = NativeImportInput.ForObjectManager(settings, dataGridInputReaderProvider);
			var contextAndExecutorDto = CreateMassImportContextAndPipelineExecutor(
				baseContext,
				settings,
				Relativity.MassImport.Core.Constants.SystemNames.ObjectManager,
				Relativity.MassImport.Core.Constants.ImportType.Natives,
				helper);
			var pipelineBuilder = new NativePipelineBuilderForObjectManager(contextAndExecutorDto.PipelineExecutor, APMClient);
			var pipeline = pipelineBuilder.BuildPipeline(contextAndExecutorDto.MassImportContext, loadStagingTablesAction);
			var results = ExecuteImport(pipeline, input, input.Settings, contextAndExecutorDto);
			return results;
		}

		public static MassImportManagerBase.MassImportResults ImportObjectsForObjectManager(BaseContext baseContext, Relativity.MassImport.DTO.ObjectLoadInfo settings, bool returnAffectedArtifactIDs, Action<TableNames> loadStagingTablesAction, IHelper helper)
		{
			var contextAndExecutorDto = CreateMassImportContextAndPipelineExecutor(
				baseContext,
				settings,
				Relativity.MassImport.Core.Constants.SystemNames.ObjectManager,
				Relativity.MassImport.Core.Constants.ImportType.Objects,
				helper);
			var pipelineBuilder = new ObjectsPipelineBuilderForObjectManagerAndRSAPI(contextAndExecutorDto.PipelineExecutor, APMClient);
			var pipeline = pipelineBuilder.BuildPipeline(contextAndExecutorDto.MassImportContext, loadStagingTablesAction);
			var input = ObjectImportInput.ForObjectManager(settings, returnAffectedArtifactIDs);
			var results = ExecuteImport(pipeline, input, input.Settings, contextAndExecutorDto);
			return results;
		}

		private static MassImportManagerBase.MassImportResults ExecuteImport<T>(
			IPipelineStage<T, MassImportManagerBase.MassImportResults> pipeline,
			T input, Relativity.MassImport.DTO.NativeLoadInfo settings,
			ContextAndExecutorDto contextAndExecutor)
		{
			var logger = contextAndExecutor.MassImportContext.Logger;
			var result = new MassImportManagerBase.MassImportResults();

			var importTime = Stopwatch.StartNew();
			try
			{
				result = contextAndExecutor.PipelineExecutor.Execute(pipeline, input);
			}
			catch (MassImportExecutionException ex)
			{
				logger.LogError(
					ex,
					"Mass Import failed. Error occured while executing '{stageName}'. Category: '{errorCategory}'",
				ex.StageName,
					ex.ErrorCategory);
				TraceHelper.SetStatusError(Activity.Current, $"Mass Import failed. Error occured while executing '{ex.StageName}'. Category: '{ex.ErrorCategory}':{ex.Message}", ex);
				result.ExceptionDetail =
					MassImportExceptionHandler.ConvertMassImportExceptionToSoapExceptionDetail(ex, settings.RunID);
			}
			catch (System.Exception ex)
			{
				logger.LogFatal(ex, "Mass Import failed. Unhandled Exception occured.");
				TraceHelper.SetStatusError(Activity.Current, $"Mass Import failed. Unhandled Exception occured: {ex.Message}", ex);
				result.ExceptionDetail =
					MassImportExceptionHandler.ConvertExceptionToSoapExceptionDetail(ex, settings.RunID);
			}

			importTime.Stop();

			var massImportMetric = new MassImportMetrics(logger, APMClient);
			massImportMetric.SendBatchCompleted(
				settings.RunID,
				importTime.ElapsedMilliseconds,
				contextAndExecutor.MassImportContext.JobDetails.ImportType,
				contextAndExecutor.MassImportContext.JobDetails.ClientSystemName,
				result,
				contextAndExecutor.MassImportContext.ImportMeasurements);

			ITelemetryPublisher telemetryPublisher = new ApmTelemetryPublisher(APMClient);
			IRelEyeMetricsService relEyeMetricsService = new RelEyeMetricsService(telemetryPublisher);
			IEventsBuilder eventsBuilder = new EventsBuilder();
			var batchCompletedEvent = eventsBuilder.BuildJobBatchCompletedEvent(result, contextAndExecutor.MassImportContext.JobDetails.ImportType);
			relEyeMetricsService.PublishEvent(batchCompletedEvent);

			return result;
		}

		private static ContextAndExecutorDto CreateMassImportContextAndPipelineExecutor(
			BaseContext baseContext,
			Relativity.MassImport.DTO.NativeLoadInfo settings,
			string clientName,
			string importType,
			IHelper helper)
		{
			var tableNames = new TableNames(settings.RunID);
			settings.RunID = tableNames.RunId; // tableNames generates runID if it was empty
			var loggingContext = new LoggingContext(settings.RunID, clientName);
			int caseSystemArtifactID = SystemArtifactCache.Instance.RetrieveArtifactIDByIdentifier(baseContext, SystemArtifact.System);
			var jobDetails = new MassImportJobDetails(tableNames, clientName, importType);
			var massImportContext = new MassImportContext(
				baseContext,
				loggingContext,
				jobDetails,
				caseSystemArtifactID,
				helper);
			var pipelineExecutor = CreatePipelineExecutor(loggingContext, massImportContext.ImportMeasurements);
			return new ContextAndExecutorDto(massImportContext, pipelineExecutor);
		}

		private static IPipelineExecutor CreatePipelineExecutor(LoggingContext loggingContext, ImportMeasurements importMeasurements)
		{
			IPipelineExecutor executor = new PipelineExecutor();
			executor = new PipelineExecutorImportMeasurementsDecorator(executor, importMeasurements);
			executor = new PipelineExecutorErrorHandlingDecorator(executor, loggingContext);
			return executor;
		}

		private class ContextAndExecutorDto
		{
			public MassImportContext MassImportContext { get; }
			public IPipelineExecutor PipelineExecutor { get; }

			public ContextAndExecutorDto(MassImportContext context, IPipelineExecutor executor)
			{
				MassImportContext = context;
				PipelineExecutor = executor;
			}
		}
	}
}