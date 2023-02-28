// <copyright file="MetricsInterceptor.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry.Events;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry.MetricsEventsBuilders;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using RelEyeAttributes = global::DataTransfer.Legacy.MassImport.RelEyeTelemetry.TelemetryConstants.AttributeNames;

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	/// <summary> 
	/// Measure execution time of intercepted method and publish all gathered in the meantime metrics. Only Task[Response] and ValueResponse return type can be intercepted. 
	/// </summary> 
	public class MetricsInterceptor : InterceptorBase
	{
		private readonly Func<IMetricsContext> _metricsContextFactory;
		private readonly IRelEyeMetricsService _relEyeMetricsService;
		private readonly IEventsBuilder _eventsBuilder;
		private Stopwatch _stopwatch;

		/// <summary> 
		/// Initializes a new instance of the <see cref="MetricsInterceptor"/> class. 
		/// </summary> 
		/// <param name="logger"></param> 
		/// <param name="metricsContextFactory"></param>
		/// <param name="relEyeMetricsService"></param>
		/// <param name="eventsBuilder"></param> 
		public MetricsInterceptor(IAPILog logger, Func<IMetricsContext> metricsContextFactory, IRelEyeMetricsService relEyeMetricsService, IEventsBuilder eventsBuilder) : base(logger)
		{
			_metricsContextFactory = metricsContextFactory;
			_relEyeMetricsService = relEyeMetricsService ?? throw new ArgumentNullException(nameof(relEyeMetricsService));
			_eventsBuilder = eventsBuilder ?? throw new ArgumentNullException(nameof(eventsBuilder));
		}

		/// <inheritdoc /> 
		public override void ExecuteBefore(IInvocation invocation)
		{
			_stopwatch = Stopwatch.StartNew();
		}

		/// <inheritdoc /> 
		public override async Task ExecuteAfter(IInvocation invocation, dynamic returnValue)
		{
			try
			{
				await LogMetricsAsync(invocation);
			}
			catch (Exception ex)
			{
				// Exception in interceptor should not break current request, no rethrow, only log the error 
				Logger.LogError(ex, "There was an error in MetricsInterceptor during call {method} - {message}", invocation.Method.Name, ex.Message);
				TraceHelper.SetStatusError(Activity.Current, $"There was an error in MetricsInterceptor during call {invocation.Method.Name} - {ex.Message}", ex);
			}
		}
		private async Task LogMetricsAsync(IInvocation invocation)
		{
			_stopwatch.Stop();
			var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;

			var parameters = invocation.Method.GetParameters();
			if (parameters.Length != invocation.Arguments.Length)
			{
				return;
			}
			
			var arguments = InterceptorHelper.GetFunctionArgumentsFrom(invocation);
			var generalMetrics = _metricsContextFactory.Invoke();
			generalMetrics.PushProperty("GeneralMetrics", "1");
			for (var i = 0; i < parameters.Length; i++)
			{
				var currentParameter = parameters[i];
				var invocationArgument = invocation.Arguments[i];

				if (invocationArgument == null)
				{
					generalMetrics.PushProperty(currentParameter.Name, "null");
					continue;
				}

				if (Attribute.IsDefined(currentParameter, typeof(SensitiveDataAttribute)))
				{
					var data = invocationArgument.ToString();
					PushSensitiveDataMetrics(generalMetrics, currentParameter.Name, data);
					continue;
				}

				if (Attribute.IsDefined(currentParameter, typeof(AuditExportDataAttribute)))
				{
					var exportStatistics = (ExportStatistics) invocationArgument;
					PushExportStatisticsMetrics(generalMetrics, exportStatistics);
					arguments.TryGetValue("correlationID", out string correlationID);
					arguments.TryGetValue("workspaceID", out string workspaceIDString);
					int.TryParse(workspaceIDString, out int workspaceID);
					EventGeneralStatistics @event = _eventsBuilder.BuildGeneralStatisticsEvent(correlationID, workspaceID);
					AddAttributesToStatisticsEvent(@event, exportStatistics);
					_relEyeMetricsService.PublishEvent(@event);
					continue;
				}

				if (Attribute.IsDefined(currentParameter, typeof(AuditObjectImportDataAttribute)))
				{
					var objectImportStatistics = (ObjectImportStatistics) invocationArgument;
					PushObjectImportStatisticsMetrics(generalMetrics, objectImportStatistics);
					arguments.TryGetValue("runID", out string runID);
					arguments.TryGetValue("workspaceID", out string workspaceIDString);
					int.TryParse(workspaceIDString, out int workspaceID);
					EventGeneralStatistics @event = _eventsBuilder.BuildGeneralStatisticsEvent(runID, workspaceID);
					AddAttributesToStatisticsEvent(@event, objectImportStatistics);
					_relEyeMetricsService.PublishEvent(@event);
					continue;
				}

				if (Attribute.IsDefined(currentParameter, typeof(AuditImageImportDataAttribute)))
				{
					var imageImportStatistics = (ImageImportStatistics) invocationArgument;
					PushImageImportStatisticsMetrics(generalMetrics, imageImportStatistics);
					arguments.TryGetValue("runID", out string runID);
					arguments.TryGetValue("workspaceID", out string workspaceIDString);
					int.TryParse(workspaceIDString, out int workspaceID);
					EventGeneralStatistics @event = _eventsBuilder.BuildGeneralStatisticsEvent(runID, workspaceID);
					AddAttributesToStatisticsEvent(@event, imageImportStatistics);
					_relEyeMetricsService.PublishEvent(@event);
					continue;
				}

				if (Attribute.IsDefined(currentParameter, typeof(ObjectLoadInfoDataAttribute)))
				{
					var objectLoadInfo = (SDK.ImportExport.V1.Models.ObjectLoadInfo) invocationArgument;
					PushObjectLoadInfoMetrics(generalMetrics, objectLoadInfo);
					continue;
				}

				if (Attribute.IsDefined(currentParameter, typeof(NativeLoadInfoDataAttribute)))
				{
					var nativeLoadInfo = (SDK.ImportExport.V1.Models.NativeLoadInfo) invocationArgument;
					PushNativeLoadInfoMetrics(generalMetrics, nativeLoadInfo);
					continue;
				}

				if (Attribute.IsDefined(currentParameter, typeof(ImageLoadInfoDataAttribute)))
				{
					var imageLoadInfo = (SDK.ImportExport.V1.Models.ImageLoadInfo) invocationArgument;
					PushImageLoadInfoMetrics(generalMetrics, imageLoadInfo);
					continue;
				}

				generalMetrics.PushProperty(currentParameter.Name, invocationArgument.ToString());
			}

			generalMetrics.PushProperty($"TargetType", invocation.TargetType.Name);
			generalMetrics.PushProperty($"Method", invocation.Method.Name);
			generalMetrics.PushProperty($"ElapsedMilliseconds", elapsedMilliseconds);
			await generalMetrics.Publish();
		}

		private void PushSensitiveDataMetrics(IMetricsContext metrics, string name, string value)
		{
			metrics.PushProperty("SensitiveDataMetrics", "1");

			var hashValue = InterceptorHelper.HashValue(value);

			metrics.PushProperty(name, hashValue);
		}

		private void PushExportStatisticsMetrics(IMetricsContext metrics, ExportStatistics exportStatistics)
		{
			metrics.PushProperty("ExportStatisticsMetrics", "1");
			metrics.PushProperty(nameof(exportStatistics.Type), exportStatistics.Type);
			metrics.PushProperty(nameof(exportStatistics.Fields), string.Join(",", exportStatistics.Fields));
			metrics.PushProperty("FieldsCount", exportStatistics.Fields.Length.ToString());
			metrics.PushProperty(nameof(exportStatistics.DestinationFilesystemFolder), exportStatistics.DestinationFilesystemFolder);
			metrics.PushProperty(nameof(exportStatistics.OverwriteFiles), exportStatistics.OverwriteFiles.ToString());
			metrics.PushProperty(nameof(exportStatistics.VolumePrefix), exportStatistics.VolumePrefix);
			metrics.PushProperty(nameof(exportStatistics.VolumeMaxSize), exportStatistics.VolumeMaxSize.ToString());
			metrics.PushProperty(nameof(exportStatistics.SubdirectoryImagePrefix), exportStatistics.SubdirectoryImagePrefix);
			metrics.PushProperty(nameof(exportStatistics.SubdirectoryNativePrefix), exportStatistics.SubdirectoryNativePrefix);
			metrics.PushProperty(nameof(exportStatistics.SubdirectoryTextPrefix), exportStatistics.SubdirectoryTextPrefix);
			metrics.PushProperty(nameof(exportStatistics.SubdirectoryStartNumber), exportStatistics.SubdirectoryStartNumber.ToString());
			metrics.PushProperty(nameof(exportStatistics.SubdirectoryMaxFileCount), exportStatistics.SubdirectoryMaxFileCount.ToString());
			metrics.PushProperty(nameof(exportStatistics.FilePathSettings), exportStatistics.FilePathSettings);
			metrics.PushProperty(nameof(exportStatistics.Delimiter), exportStatistics.Delimiter.ToString());
			metrics.PushProperty(nameof(exportStatistics.Bound), exportStatistics.Bound.ToString());
			metrics.PushProperty(nameof(exportStatistics.NewlineProxy), exportStatistics.NewlineProxy.ToString());
			metrics.PushProperty(nameof(exportStatistics.MultiValueDelimiter), exportStatistics.MultiValueDelimiter.ToString());
			metrics.PushProperty(nameof(exportStatistics.NestedValueDelimiter), exportStatistics.NestedValueDelimiter.ToString());
			metrics.PushProperty(nameof(exportStatistics.TextAndNativeFilesNamedAfterFieldID), exportStatistics.TextAndNativeFilesNamedAfterFieldID.ToString());
			metrics.PushProperty(nameof(exportStatistics.AppendOriginalFilenames), exportStatistics.AppendOriginalFilenames.ToString());
			metrics.PushProperty(nameof(exportStatistics.ExportImages), exportStatistics.ExportImages.ToString());
			metrics.PushProperty(nameof(exportStatistics.ImageLoadFileFormat), Enum.GetName(typeof(ImageLoadFileFormatType), exportStatistics.ImageLoadFileFormat));
			metrics.PushProperty(nameof(exportStatistics.ImageFileType), Enum.GetName(typeof(ImageFileExportType), exportStatistics.ImageFileType));
			metrics.PushProperty(nameof(exportStatistics.ExportNativeFiles), exportStatistics.ExportNativeFiles.ToString());
			metrics.PushProperty(nameof(exportStatistics.MetadataLoadFileFormat), Enum.GetName(typeof(LoadFileFormat), exportStatistics.MetadataLoadFileFormat));
			metrics.PushProperty(nameof(exportStatistics.MetadataLoadFileEncodingCodePage), exportStatistics.MetadataLoadFileEncodingCodePage.ToString());
			metrics.PushProperty(nameof(exportStatistics.ExportTextFieldAsFiles), exportStatistics.ExportTextFieldAsFiles.ToString());
			metrics.PushProperty(nameof(exportStatistics.ExportedTextFileEncodingCodePage), exportStatistics.ExportedTextFileEncodingCodePage.ToString());
			metrics.PushProperty(nameof(exportStatistics.ExportedTextFieldID), exportStatistics.ExportedTextFieldID.ToString());
			metrics.PushProperty(nameof(exportStatistics.ExportMultipleChoiceFieldsAsNested), exportStatistics.ExportMultipleChoiceFieldsAsNested.ToString());
			metrics.PushProperty(nameof(exportStatistics.TotalFileBytesExported), exportStatistics.TotalFileBytesExported.ToString());
			metrics.PushProperty(nameof(exportStatistics.TotalMetadataBytesExported), exportStatistics.TotalMetadataBytesExported.ToString());
			metrics.PushProperty(nameof(exportStatistics.ErrorCount), exportStatistics.ErrorCount.ToString());
			metrics.PushProperty(nameof(exportStatistics.WarningCount), exportStatistics.WarningCount.ToString());
			metrics.PushProperty(nameof(exportStatistics.DocumentExportCount), exportStatistics.DocumentExportCount.ToString());
			metrics.PushProperty(nameof(exportStatistics.FileExportCount), exportStatistics.FileExportCount.ToString());
			metrics.PushProperty(nameof(exportStatistics.ImagesToExport), Enum.GetName(typeof(ImagesToExportType), exportStatistics.ImagesToExport));
			metrics.PushProperty(nameof(exportStatistics.ProductionPrecedence), string.Join(",", exportStatistics.ProductionPrecedence));
			metrics.PushProperty("ProductionPrecedenceCount", exportStatistics.ProductionPrecedence.Length.ToString());
			metrics.PushProperty(nameof(exportStatistics.DataSourceArtifactID), exportStatistics.DataSourceArtifactID.ToString());
			metrics.PushProperty(nameof(exportStatistics.SourceRootFolderID), exportStatistics.SourceRootFolderID.ToString());
			metrics.PushProperty(nameof(exportStatistics.RunTimeInMilliseconds), exportStatistics.RunTimeInMilliseconds.ToString());
			metrics.PushProperty(nameof(exportStatistics.CopyFilesFromRepository), exportStatistics.CopyFilesFromRepository.ToString());
			metrics.PushProperty(nameof(exportStatistics.StartExportAtDocumentNumber), exportStatistics.StartExportAtDocumentNumber.ToString());
			metrics.PushProperty(nameof(exportStatistics.VolumeStartNumber), exportStatistics.VolumeStartNumber.ToString());
			metrics.PushProperty(nameof(exportStatistics.ArtifactTypeID), exportStatistics.ArtifactTypeID.ToString());
			metrics.PushProperty(nameof(exportStatistics.SubdirectoryPDFPrefix), exportStatistics.SubdirectoryPDFPrefix);
			metrics.PushProperty(nameof(exportStatistics.ExportSearchablePDFs), exportStatistics.ExportSearchablePDFs.ToString());
		}

		private void PushObjectImportStatisticsMetrics(IMetricsContext metrics, ObjectImportStatistics objectImportStatistics)
		{
			metrics.PushProperty("ObjectImportStatisticsMetrics", "1");
			metrics.PushProperty(nameof(objectImportStatistics.ArtifactTypeID), objectImportStatistics.ArtifactTypeID.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.Delimiter), objectImportStatistics.Delimiter.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.Bound), objectImportStatistics.Bound.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.NewlineProxy), objectImportStatistics.NewlineProxy.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.MultiValueDelimiter), objectImportStatistics.MultiValueDelimiter.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.LoadFileEncodingCodePageID), objectImportStatistics.LoadFileEncodingCodePageID.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.ExtractedTextFileEncodingCodePageID), objectImportStatistics.ExtractedTextFileEncodingCodePageID.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.FolderColumnName), objectImportStatistics.FolderColumnName);
			metrics.PushProperty(nameof(objectImportStatistics.FileFieldColumnName), objectImportStatistics.FileFieldColumnName);
			metrics.PushProperty(nameof(objectImportStatistics.ExtractedTextPointsToFile), objectImportStatistics.ExtractedTextPointsToFile.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.NumberOfChoicesCreated), objectImportStatistics.NumberOfChoicesCreated.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.NumberOfFoldersCreated), objectImportStatistics.NumberOfFoldersCreated.ToString());

			var mapping = objectImportStatistics.FieldsMapped.Select(fields => string.Join(",", fields)).ToList();
			metrics.PushProperty(nameof(objectImportStatistics.FieldsMapped), string.Join(";", mapping));
			metrics.PushProperty("FieldsMappedCount", objectImportStatistics.FieldsMapped.Length.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.NestedValueDelimiter), objectImportStatistics.NestedValueDelimiter.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.BatchSizes), string.Join(",", objectImportStatistics.BatchSizes));
			metrics.PushProperty("BatchSizesCount", objectImportStatistics.BatchSizes.Length.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.RepositoryConnection), Enum.GetName(typeof(RepositoryConnectionType), objectImportStatistics.RepositoryConnection));
			metrics.PushProperty(nameof(objectImportStatistics.Overwrite), Enum.GetName(typeof(OverwriteType), objectImportStatistics.Overwrite));
			metrics.PushProperty(nameof(objectImportStatistics.OverlayIdentifierFieldArtifactID), objectImportStatistics.OverlayIdentifierFieldArtifactID.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.DestinationFolderArtifactID), objectImportStatistics.DestinationFolderArtifactID.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.LoadFileName), objectImportStatistics.LoadFileName);
			metrics.PushProperty(nameof(objectImportStatistics.StartLine), objectImportStatistics.StartLine.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.FilesCopiedToRepository), objectImportStatistics.FilesCopiedToRepository);
			metrics.PushProperty(nameof(objectImportStatistics.TotalFileSize), objectImportStatistics.TotalFileSize.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.TotalMetadataBytes), objectImportStatistics.TotalMetadataBytes.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.NumberOfDocumentsCreated), objectImportStatistics.NumberOfDocumentsCreated.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.NumberOfDocumentsUpdated), objectImportStatistics.NumberOfDocumentsUpdated.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.NumberOfFilesLoaded), objectImportStatistics.NumberOfFilesLoaded.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.NumberOfErrors), objectImportStatistics.NumberOfErrors.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.NumberOfWarnings), objectImportStatistics.NumberOfWarnings.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.RunTimeInMilliseconds), objectImportStatistics.RunTimeInMilliseconds.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.SendNotification), objectImportStatistics.SendNotification.ToString());
			metrics.PushProperty(nameof(objectImportStatistics.OverlayBehavior), objectImportStatistics.OverlayBehavior.HasValue ? Enum.GetName(typeof(OverlayBehavior), objectImportStatistics.OverlayBehavior) : "null");
		}

		private void PushImageImportStatisticsMetrics(IMetricsContext metrics, ImageImportStatistics imageImportStatistics)
		{
			metrics.PushProperty("ImageImportStatisticsMetrics", "1");
			metrics.PushProperty(nameof(imageImportStatistics.ExtractedTextReplaced), imageImportStatistics.ExtractedTextReplaced.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.SupportImageAutoNumbering), imageImportStatistics.SupportImageAutoNumbering.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.DestinationProductionArtifactID), imageImportStatistics.DestinationProductionArtifactID.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.ExtractedTextDefaultEncodingCodePageID), imageImportStatistics.ExtractedTextDefaultEncodingCodePageID.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.BatchSizes), string.Join(",", imageImportStatistics.BatchSizes));
			metrics.PushProperty("BatchSizesCount", imageImportStatistics.BatchSizes.Length.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.RepositoryConnection), Enum.GetName(typeof(RepositoryConnectionType), imageImportStatistics.RepositoryConnection));
			metrics.PushProperty(nameof(imageImportStatistics.Overwrite), Enum.GetName(typeof(OverwriteType), imageImportStatistics.Overwrite));
			metrics.PushProperty(nameof(imageImportStatistics.OverlayIdentifierFieldArtifactID), imageImportStatistics.OverlayIdentifierFieldArtifactID.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.DestinationFolderArtifactID), imageImportStatistics.DestinationFolderArtifactID.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.LoadFileName), imageImportStatistics.LoadFileName);
			metrics.PushProperty(nameof(imageImportStatistics.StartLine), imageImportStatistics.StartLine.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.FilesCopiedToRepository), imageImportStatistics.FilesCopiedToRepository);
			metrics.PushProperty(nameof(imageImportStatistics.TotalFileSize), imageImportStatistics.TotalFileSize.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.TotalMetadataBytes), imageImportStatistics.TotalMetadataBytes.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.NumberOfDocumentsCreated), imageImportStatistics.NumberOfDocumentsCreated.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.NumberOfDocumentsUpdated), imageImportStatistics.NumberOfDocumentsUpdated.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.NumberOfFilesLoaded), imageImportStatistics.NumberOfFilesLoaded.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.NumberOfErrors), imageImportStatistics.NumberOfErrors.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.NumberOfWarnings), imageImportStatistics.NumberOfWarnings.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.RunTimeInMilliseconds), imageImportStatistics.RunTimeInMilliseconds.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.SendNotification), imageImportStatistics.SendNotification.ToString());
			metrics.PushProperty(nameof(imageImportStatistics.OverlayBehavior), imageImportStatistics.OverlayBehavior.HasValue ? Enum.GetName(typeof(OverlayBehavior), imageImportStatistics.OverlayBehavior) : "null");
		}

		private void PushObjectLoadInfoMetrics(IMetricsContext metrics, SDK.ImportExport.V1.Models.ObjectLoadInfo objectLoadInfo)
		{
			metrics.PushProperty("ObjectLoadInfoMetrics", "1");
			metrics.PushProperty(nameof(objectLoadInfo.ArtifactTypeID), objectLoadInfo.ArtifactTypeID.ToString());

			PushNativeLoadInfoMetrics(metrics, objectLoadInfo);
		}

		private void PushNativeLoadInfoMetrics(IMetricsContext metrics, SDK.ImportExport.V1.Models.NativeLoadInfo nativeLoadInfo)
		{
			metrics.PushProperty("NativeLoadInfoMetrics", "1");
			var range = nativeLoadInfo.Range;
			if (range != null)
			{
				metrics.PushProperty(nameof(nativeLoadInfo.Range.StartIndex),
					nativeLoadInfo.Range.StartIndex.ToString());
				metrics.PushProperty(nameof(nativeLoadInfo.Range.Count), nativeLoadInfo.Range.Count.ToString());
			}

			metrics.PushProperty(nameof(nativeLoadInfo.Overlay), Enum.GetName(typeof(OverwriteType), nativeLoadInfo.Overlay));
			metrics.PushProperty(nameof(nativeLoadInfo.Repository), nativeLoadInfo.Repository);
			metrics.PushProperty(nameof(nativeLoadInfo.RunID), nativeLoadInfo.RunID);
			metrics.PushProperty(nameof(nativeLoadInfo.DataFileName), nativeLoadInfo.DataFileName);
			metrics.PushProperty(nameof(nativeLoadInfo.UseBulkDataImport), nativeLoadInfo.UseBulkDataImport.ToString());
			metrics.PushProperty(nameof(nativeLoadInfo.UploadFiles), nativeLoadInfo.UploadFiles.ToString());
			metrics.PushProperty(nameof(nativeLoadInfo.CodeFileName), nativeLoadInfo.CodeFileName);
			metrics.PushProperty(nameof(nativeLoadInfo.ObjectFileName), nativeLoadInfo.ObjectFileName);
			metrics.PushProperty(nameof(nativeLoadInfo.DataGridFileName), nativeLoadInfo.DataGridFileName);
			metrics.PushProperty(nameof(nativeLoadInfo.DataGridOffsetFileName), nativeLoadInfo.DataGridOffsetFileName);
			metrics.PushProperty(nameof(nativeLoadInfo.DisableUserSecurityCheck), nativeLoadInfo.DisableUserSecurityCheck.ToString());
			metrics.PushProperty(nameof(nativeLoadInfo.OnBehalfOfUserToken), nativeLoadInfo.OnBehalfOfUserToken);
			metrics.PushProperty(nameof(nativeLoadInfo.AuditLevel), Enum.GetName(typeof(ImportAuditLevel), nativeLoadInfo.AuditLevel));
			metrics.PushProperty(nameof(nativeLoadInfo.BulkLoadFileFieldDelimiter), nativeLoadInfo.BulkLoadFileFieldDelimiter);
			metrics.PushProperty(nameof(nativeLoadInfo.OverlayArtifactID), nativeLoadInfo.OverlayArtifactID.ToString());
			metrics.PushProperty(nameof(nativeLoadInfo.OverlayBehavior), Enum.GetName(typeof(OverlayBehavior), nativeLoadInfo.OverlayBehavior));
			metrics.PushProperty(nameof(nativeLoadInfo.LinkDataGridRecords), nativeLoadInfo.LinkDataGridRecords.ToString());
			metrics.PushProperty(nameof(nativeLoadInfo.LoadImportedFullTextFromServer), nativeLoadInfo.LoadImportedFullTextFromServer.ToString());
			metrics.PushProperty(nameof(nativeLoadInfo.KeyFieldArtifactID), nativeLoadInfo.KeyFieldArtifactID.ToString());
			metrics.PushProperty(nameof(nativeLoadInfo.RootFolderID), nativeLoadInfo.RootFolderID.ToString());
			metrics.PushProperty(nameof(nativeLoadInfo.MoveDocumentsInAppendOverlayMode), nativeLoadInfo.MoveDocumentsInAppendOverlayMode.ToString());
			metrics.PushProperty(nameof(nativeLoadInfo.ExecutionSource), Enum.GetName(typeof(ExecutionSource), nativeLoadInfo.ExecutionSource));
			metrics.PushProperty(nameof(nativeLoadInfo.Billable), nativeLoadInfo.Billable.ToString());
            metrics.PushProperty(nameof(nativeLoadInfo.OverrideReferentialLinksRestriction), nativeLoadInfo.OverrideReferentialLinksRestriction);
        }

		private void PushImageLoadInfoMetrics(IMetricsContext metrics, SDK.ImportExport.V1.Models.ImageLoadInfo imageLoadInfo)
		{
			metrics.PushProperty("ImageLoadInfoMetrics", "1");
			metrics.PushProperty(nameof(imageLoadInfo.DisableUserSecurityCheck), imageLoadInfo.DisableUserSecurityCheck);
			metrics.PushProperty(nameof(imageLoadInfo.RunID), imageLoadInfo.RunID);
			metrics.PushProperty(nameof(imageLoadInfo.Overlay), Enum.GetName(typeof(OverwriteType), imageLoadInfo.Overlay));
			metrics.PushProperty(nameof(imageLoadInfo.Repository), imageLoadInfo.Repository);
			metrics.PushProperty(nameof(imageLoadInfo.UseBulkDataImport), imageLoadInfo.UseBulkDataImport);
			metrics.PushProperty(nameof(imageLoadInfo.UploadFullText), imageLoadInfo.UploadFullText);
			metrics.PushProperty(nameof(imageLoadInfo.BulkFileName), imageLoadInfo.BulkFileName);
			metrics.PushProperty(nameof(imageLoadInfo.DataGridFileName), imageLoadInfo.DataGridFileName);
			metrics.PushProperty(nameof(imageLoadInfo.KeyFieldArtifactID), imageLoadInfo.KeyFieldArtifactID);
			metrics.PushProperty(nameof(imageLoadInfo.DestinationFolderArtifactID), imageLoadInfo.DestinationFolderArtifactID);
			metrics.PushProperty(nameof(imageLoadInfo.AuditLevel), Enum.GetName(typeof(ImportAuditLevel), imageLoadInfo.AuditLevel));
			metrics.PushProperty(nameof(imageLoadInfo.OverlayArtifactID), imageLoadInfo.OverlayArtifactID);
			metrics.PushProperty(nameof(imageLoadInfo.ExecutionSource), Enum.GetName(typeof(ExecutionSource), imageLoadInfo.ExecutionSource));
			metrics.PushProperty(nameof(imageLoadInfo.Billable), imageLoadInfo.Billable);
			metrics.PushProperty(nameof(imageLoadInfo.HasPDF), imageLoadInfo.HasPDF);
			metrics.PushProperty(nameof(imageLoadInfo.OverrideReferentialLinksRestriction), imageLoadInfo.OverrideReferentialLinksRestriction);
		}

		private void AddAttributesToStatisticsEvent(EventGeneralStatistics eventGeneral, ObjectImportStatistics objectImportStatistics)
		{
			eventGeneral.Attributes[RelEyeAttributes.DocumentsCreatedCount] = objectImportStatistics.NumberOfDocumentsCreated;
			eventGeneral.Attributes[RelEyeAttributes.DocumentsUpdatedCount] = objectImportStatistics.NumberOfDocumentsUpdated;
			eventGeneral.Attributes[RelEyeAttributes.ChoicesCreatedCount] = objectImportStatistics.NumberOfChoicesCreated;
			eventGeneral.Attributes[RelEyeAttributes.FoldersCreatedCount] = objectImportStatistics.NumberOfFoldersCreated;
			eventGeneral.Attributes[RelEyeAttributes.ErrorsCount] = objectImportStatistics.NumberOfErrors;
			eventGeneral.Attributes[RelEyeAttributes.WarningsCount] = objectImportStatistics.NumberOfWarnings;
			eventGeneral.Attributes[RelEyeAttributes.FilesLoadedCount] = objectImportStatistics.NumberOfFilesLoaded;
		}

		private void AddAttributesToStatisticsEvent(EventGeneralStatistics eventGeneral, ImageImportStatistics imageImportStatistics)
		{
			eventGeneral.Attributes[RelEyeAttributes.DocumentsCreatedCount] = imageImportStatistics.NumberOfDocumentsCreated;
			eventGeneral.Attributes[RelEyeAttributes.DocumentsUpdatedCount] = imageImportStatistics.NumberOfDocumentsUpdated;
			eventGeneral.Attributes[RelEyeAttributes.ErrorsCount] = imageImportStatistics.NumberOfErrors;
			eventGeneral.Attributes[RelEyeAttributes.WarningsCount] = imageImportStatistics.NumberOfWarnings;
			eventGeneral.Attributes[RelEyeAttributes.FilesLoadedCount] = imageImportStatistics.NumberOfFilesLoaded;
		}

		private void AddAttributesToStatisticsEvent(EventGeneralStatistics eventGeneral, ExportStatistics exportStatistics)
		{
			eventGeneral.Attributes[RelEyeAttributes.ErrorsCount] = exportStatistics.ErrorCount;
			eventGeneral.Attributes[RelEyeAttributes.WarningsCount] = exportStatistics.WarningCount;
		}
	}
}