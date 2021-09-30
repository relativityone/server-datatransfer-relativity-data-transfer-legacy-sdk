// <copyright file="MetricsInterceptor.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Metrics;

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	/// <summary> 
	/// Measure execution time of intercepted method and publish all gathered in the meantime metrics. Only Task[Response] and ValueResponse return type can be intercepted. 
	/// </summary> 
	public class MetricsInterceptor : InterceptorBase
	{
		private readonly Func<IMetricsContext> _metricsContextFactory;
		private Stopwatch _stopwatch;

		/// <summary> 
		/// Initializes a new instance of the <see cref="MetricsInterceptor"/> class. 
		/// </summary> 
		/// <param name="logger"></param> 
		/// <param name="metricsContextFactory"></param> 
		public MetricsInterceptor(IAPILog logger, Func<IMetricsContext> metricsContextFactory) : base(logger)
		{
			_metricsContextFactory = metricsContextFactory;
		}

		/// <inheritdoc /> 
		public override void ExecuteBefore(IInvocation invocation)
		{
			_stopwatch = Stopwatch.StartNew();
		}

		/// <inheritdoc /> 
		public override async Task ExecuteAfter(IInvocation invocation, dynamic returnValue)
		{
			_stopwatch.Stop();
			var elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;
			
			var parameters = invocation.Method.GetParameters();
			if (parameters.Length != invocation.Arguments.Length)
			{
				return;
			}
			
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
					var exportStatistics = (ExportStatistics)invocationArgument;
					PushExportStatisticsMetrics(generalMetrics, exportStatistics);
					continue;
				}

				if (Attribute.IsDefined(currentParameter, typeof(AuditObjectImportDataAttribute)))
				{
					var objectImportStatistics = (ObjectImportStatistics)invocationArgument;
					PushObjectImportStatisticsMetrics(generalMetrics, objectImportStatistics);
					continue;
				}

				if (Attribute.IsDefined(currentParameter, typeof(AuditImageImportDataAttribute)))
				{
					var imageImportStatistics = (ImageImportStatistics)invocationArgument;
					PushImageImportStatisticsMetrics(generalMetrics, imageImportStatistics);
					continue;
				}

				if (Attribute.IsDefined(currentParameter, typeof(ObjectLoadInfoDataAttribute)))
				{
					var objectLoadInfo = (SDK.ImportExport.V1.Models.ObjectLoadInfo)invocationArgument;
					PushObjectLoadInfoMetrics(generalMetrics, objectLoadInfo);

					await SendFieldInfoMetrics(invocation, parameters, objectLoadInfo, elapsedMilliseconds);

					continue;
				}

				if (Attribute.IsDefined(currentParameter, typeof(NativeLoadInfoDataAttribute)))
				{
					var nativeLoadInfo = (SDK.ImportExport.V1.Models.NativeLoadInfo)invocationArgument;
					PushNativeLoadInfoMetrics(generalMetrics, nativeLoadInfo);

					await SendFieldInfoMetrics(invocation, parameters, nativeLoadInfo, elapsedMilliseconds);

					continue;
				}

				if (Attribute.IsDefined(currentParameter, typeof(ImageLoadInfoDataAttribute)))
				{
					var imageLoadInfo = (SDK.ImportExport.V1.Models.ImageLoadInfo)invocationArgument;
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
			metrics.PushProperty(nameof(objectLoadInfo.Range.StartIndex), objectLoadInfo.Range.StartIndex.ToString());
			metrics.PushProperty(nameof(objectLoadInfo.Range.Count), objectLoadInfo.Range.Count.ToString());
			metrics.PushProperty(nameof(objectLoadInfo.Overlay), Enum.GetName(typeof(OverwriteType), objectLoadInfo.Overlay));
			metrics.PushProperty(nameof(objectLoadInfo.Repository), objectLoadInfo.Repository);
			metrics.PushProperty(nameof(objectLoadInfo.RunID), objectLoadInfo.RunID);
			metrics.PushProperty(nameof(objectLoadInfo.DataFileName), objectLoadInfo.DataFileName);
			metrics.PushProperty(nameof(objectLoadInfo.UseBulkDataImport), objectLoadInfo.UseBulkDataImport.ToString());
			metrics.PushProperty(nameof(objectLoadInfo.UploadFiles), objectLoadInfo.UploadFiles.ToString());
			metrics.PushProperty(nameof(objectLoadInfo.CodeFileName), objectLoadInfo.CodeFileName);
			metrics.PushProperty(nameof(objectLoadInfo.ObjectFileName), objectLoadInfo.ObjectFileName);
			metrics.PushProperty(nameof(objectLoadInfo.DataGridFileName), objectLoadInfo.DataGridFileName);
			metrics.PushProperty(nameof(objectLoadInfo.DataGridOffsetFileName), objectLoadInfo.DataGridOffsetFileName);
			metrics.PushProperty(nameof(objectLoadInfo.DisableUserSecurityCheck), objectLoadInfo.DisableUserSecurityCheck.ToString());
			metrics.PushProperty(nameof(objectLoadInfo.OnBehalfOfUserToken), objectLoadInfo.OnBehalfOfUserToken);
			metrics.PushProperty(nameof(objectLoadInfo.AuditLevel), Enum.GetName(typeof(ImportAuditLevel), objectLoadInfo.AuditLevel));
			metrics.PushProperty(nameof(objectLoadInfo.BulkLoadFileFieldDelimiter), objectLoadInfo.BulkLoadFileFieldDelimiter);
			metrics.PushProperty(nameof(objectLoadInfo.OverlayArtifactID), objectLoadInfo.OverlayArtifactID.ToString());
			metrics.PushProperty(nameof(objectLoadInfo.OverlayBehavior), Enum.GetName(typeof(OverlayBehavior), objectLoadInfo.OverlayBehavior));
			metrics.PushProperty(nameof(objectLoadInfo.LinkDataGridRecords), objectLoadInfo.LinkDataGridRecords.ToString());
			metrics.PushProperty(nameof(objectLoadInfo.LoadImportedFullTextFromServer), objectLoadInfo.LoadImportedFullTextFromServer.ToString());
			metrics.PushProperty(nameof(objectLoadInfo.KeyFieldArtifactID), objectLoadInfo.KeyFieldArtifactID.ToString());
			metrics.PushProperty(nameof(objectLoadInfo.RootFolderID), objectLoadInfo.RootFolderID.ToString());
			metrics.PushProperty(nameof(objectLoadInfo.MoveDocumentsInAppendOverlayMode), objectLoadInfo.MoveDocumentsInAppendOverlayMode.ToString());
			metrics.PushProperty(nameof(objectLoadInfo.ExecutionSource), Enum.GetName(typeof(ExecutionSource), objectLoadInfo.ExecutionSource));
			metrics.PushProperty(nameof(objectLoadInfo.Billable), objectLoadInfo.Billable.ToString());
		}

		private async Task SendFieldInfoMetrics(IInvocation invocation, ParameterInfo[] parameters, SDK.ImportExport.V1.Models.NativeLoadInfo objectLoadInfo, long elapsedMilliseconds)
		{
			var workspaceID = string.Empty;
			var firstParameter = parameters.First();
			if (firstParameter.Name == "workspaceID")
			{
				workspaceID = invocation.Arguments.First()?.ToString();
			}

			var correlationID = string.Empty;
			var lastParameter = parameters.Last();
			if (lastParameter.Name == "correlationID")
			{
				correlationID = invocation.Arguments.Last()?.ToString();
			}

			foreach (var fieldInfo in objectLoadInfo.MappedFields)
			{
				await SendFieldInfoMetric(fieldInfo, workspaceID, objectLoadInfo.RunID, invocation.TargetType.Name,
					invocation.Method.Name, elapsedMilliseconds, correlationID);
			}
		}

		private async Task SendFieldInfoMetric(SDK.ImportExport.V1.Models.FieldInfo fieldInfo, string workspaceID, string runID, string targetType, string method, long elapsedMilliseconds, string correlationID)
		{
			var metrics = _metricsContextFactory.Invoke();

			metrics.PushProperty("FieldInfoMetrics", "1");
			metrics.PushProperty(nameof(fieldInfo.ArtifactID), fieldInfo.ArtifactID.ToString());
			metrics.PushProperty(nameof(fieldInfo.Category), Enum.GetName(typeof(FieldCategory), fieldInfo.Category));
			metrics.PushProperty(nameof(fieldInfo.Type), Enum.GetName(typeof(FieldType), fieldInfo.Type));
			metrics.PushProperty(nameof(fieldInfo.ImportBehavior), fieldInfo.ImportBehavior.HasValue ? Enum.GetName(typeof(ImportBehaviorChoice), fieldInfo.ImportBehavior) : "null");
			metrics.PushProperty(nameof(fieldInfo.DisplayName), fieldInfo.DisplayName);
			metrics.PushProperty(nameof(fieldInfo.TextLength), fieldInfo.TextLength.ToString());
			metrics.PushProperty(nameof(fieldInfo.CodeTypeID), fieldInfo.CodeTypeID.ToString());
			metrics.PushProperty(nameof(fieldInfo.EnableDataGrid), fieldInfo.EnableDataGrid.ToString());
			metrics.PushProperty(nameof(fieldInfo.FormatString), fieldInfo.FormatString);
			metrics.PushProperty(nameof(fieldInfo.IsUnicodeEnabled), fieldInfo.IsUnicodeEnabled.ToString());

			metrics.PushProperty($"workspaceID", workspaceID);
			metrics.PushProperty($"runID", runID);
			metrics.PushProperty($"correlationID", correlationID);
			metrics.PushProperty($"TargetType", targetType);
			metrics.PushProperty($"Method", method);
			metrics.PushProperty($"ElapsedMilliseconds", elapsedMilliseconds);

			await metrics.Publish();
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
		}
	}
}