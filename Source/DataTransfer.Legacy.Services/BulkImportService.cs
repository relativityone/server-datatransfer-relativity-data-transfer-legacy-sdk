using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core;
using Relativity.Core.Exception;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Helpers.BatchCache;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using Permission = Relativity.Core.Permission;
using TelemetryConstants = DataTransfer.Legacy.MassImport.RelEyeTelemetry.TelemetryConstants;
using Relativity.API;

namespace Relativity.DataTransfer.Legacy.Services
{
	using System.Collections.Generic;
	using System.Web.UI.WebControls;

	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(DistributedTracingInterceptor))]
	public class BulkImportService : BaseService, IBulkImportService
	{
		private const string SecurityWarning =
			@"##InsufficientPermissionsForImportException##You do not have permission to run this import because the ""Disable Security Check"" field is turned on.  You must either log in as a system administrator or turn off the ""Disable Security Check"" field to run this import.";

		private const string AuditLevelWarningNonAdminsCantNoSnapshot =
			@"##InsufficientPermissionsForImportException##You do not have permission to run this import because the ""Audit Level"" field is not set to full audit mode.  You must either log in as a system administrator or set the ""Audit Level"" field to full audit mode to run this import.";

		private const string AuditLevelWarningNonAdminsCanNoSnapshot =
			@"##InsufficientPermissionsForImportException##You do not have permission to run this import because the ""Audit Level"" field is not set to Full audit mode or NoSnapshot audit mode.  You must either log in as a system administrator or set the ""Audit Level"" field to Full audit mode or NoSnapshot audit mode to run this import.";

		private const string FileSecurityWarning =
			@"##InsufficientPermissionsForImportException##You do not have permission to run this import because it uses referential links to files. You must either log in as a system administrator or change the settings to upload files to run this import.";

		private readonly MassImportManager _massImportManager;

		private readonly ISnowflakeMetrics _metrics;
		private readonly IBatchResultCache _batchResultCache;
		private readonly IHelper _helper;

		public BulkImportService(IServiceContextFactory serviceContextFactory, ISnowflakeMetrics metrics, IBatchResultCache batchResultCache, IHelper helper)
			: base(serviceContextFactory)
		{
			_massImportManager = new MassImportManager(false, helper);
			_metrics = metrics;
			_batchResultCache = batchResultCache;
			_helper = helper;
		}

		public Task<MassImportResults> BulkImportImageAsync(int workspaceID,
			SDK.ImportExport.V1.Models.ImageLoadInfo settings, bool inRepository, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.ExecutionSource, settings.ExecutionSource);
			activity?.SetTag(TelemetryConstants.AttributeNames.RunID, settings.RunID);

			IImportCoordinator coordinator = new ImageImportCoordinator(inRepository, settings);
			var runSettings = new RunSettings(
				workspaceID,
				(ExecutionSource)((int)settings.ExecutionSource),
				settings.OverrideReferentialLinksRestriction,
				settings.RunID,
				settings.BulkFileName);

			var result = BulkImport(runSettings, coordinator);

			return Task.FromResult(result);
		}

		public Task<MassImportResults> BulkImportProductionImageAsync(int workspaceID, SDK.ImportExport.V1.Models.ImageLoadInfo settings, int productionArtifactID, bool inRepository, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.ExecutionSource, settings.ExecutionSource);
			activity?.SetTag(TelemetryConstants.AttributeNames.RunID, settings.RunID);

			IImportCoordinator coordinator = new ProductionImportCoordinator(inRepository, productionArtifactID, settings);
			var runSettings = new RunSettings(
				workspaceID,
				(ExecutionSource)((int)settings.ExecutionSource),
				settings.OverrideReferentialLinksRestriction,
				settings.RunID,
				settings.BulkFileName);

			var result = BulkImport(runSettings, coordinator);

			return Task.FromResult(result);
		}

		public Task<MassImportResults> BulkImportNativeAsync(int workspaceID, SDK.ImportExport.V1.Models.NativeLoadInfo settings, bool inRepository, bool includeExtractedTextEncoding, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.ExecutionSource, settings.ExecutionSource);
			activity?.SetTag(TelemetryConstants.AttributeNames.RunID, settings.RunID);

			IImportCoordinator coordinator = new NativeImportCoordinator(inRepository, includeExtractedTextEncoding, settings);
			var runSettings = new RunSettings(
				workspaceID,
				(ExecutionSource)((int)settings.ExecutionSource),
				settings.OverrideReferentialLinksRestriction,
				settings.RunID,
				settings.DataFileName);

			var result = BulkImport(runSettings, coordinator);

			return Task.FromResult(result);
		}

		public Task<MassImportResults> BulkImportObjectsAsync(int workspaceID,
			SDK.ImportExport.V1.Models.ObjectLoadInfo settings, bool inRepository, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.ExecutionSource, settings.ExecutionSource);
			activity?.SetTag(TelemetryConstants.AttributeNames.RunID, settings.RunID);

			IImportCoordinator coordinator = new RdoImportCoordinator(inRepository, settings);
			var runSettings = new RunSettings(
				workspaceID,
				(ExecutionSource)((int)settings.ExecutionSource),
				settings.OverrideReferentialLinksRestriction,
				settings.RunID,
				settings.DataFileName);

			var result = BulkImport(runSettings, coordinator);

			return Task.FromResult(result);
		}

		private MassImportResults BulkImport(RunSettings runSettings, IImportCoordinator coordinator)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.BatchId, runSettings.BatchID);

			var serviceContext = GetBaseServiceContext(runSettings.WorkspaceID);
			var massImportManager = new MassImportManager(false, _helper);

			if (!massImportManager.HasImportPermission(serviceContext))
			{
				throw new InsufficientAccessControlListPermissions("Insufficient Permissions! Please ask your Relativity Administrator to allow you import permission.");
			}

			var nonAdminsCanNoSnapshot = Config.AllowNoSnapshotImport;
			if (!UserQuery.UserIsSystemAdmin(serviceContext.GetMasterDbServiceContext()))
			{
				if (coordinator.DisableUserSecurityCheck)
				{
					return CreateResultWithException(SecurityWarning);
				}

				if (!nonAdminsCanNoSnapshot && coordinator.AuditLevel != DataTransfer.Legacy.SDK.ImportExport.V1.Models.ImportAuditLevel.FullAudit)
				{
					return CreateResultWithException(AuditLevelWarningNonAdminsCantNoSnapshot);
				}

				if (nonAdminsCanNoSnapshot && coordinator.AuditLevel == DataTransfer.Legacy.SDK.ImportExport.V1.Models.ImportAuditLevel.NoAudit)
				{
					return CreateResultWithException(AuditLevelWarningNonAdminsCanNoSnapshot);
				}

				if (coordinator.ImportHasLinkedFiles() && Config.RestrictReferentialFileLinksOnImport && !runSettings.OverrideReferentialLinksRestriction)
				{
					return CreateResultWithException(FileSecurityWarning);
				}
			}

			var existingResult = _batchResultCache.GetCreateOrThrow(runSettings.WorkspaceID, runSettings.RunID, runSettings.BatchID);
			if (existingResult != null)
			{
				return existingResult;
			}

			var results = coordinator.RunImport(serviceContext, massImportManager);

			_metrics.LogTelemetryMetricsForImport(serviceContext, results, runSettings.ExecutionSource, runSettings.WorkspaceID);
			var returnValue = results.Map<MassImportResults>();
			_batchResultCache.Update(runSettings.WorkspaceID, runSettings.RunID, runSettings.BatchID, returnValue);
			return returnValue;
		}

		private static MassImportResults CreateResultWithException(string exceptionMessage)
		{
			var soapExceptionDetail = new Relativity.MassImport.DTO.SoapExceptionDetail(new Exception(exceptionMessage));
			return new MassImportResults
			{
				ExceptionDetail = soapExceptionDetail.Map<SDK.ImportExport.V1.Models.SoapExceptionDetail>()
			};
		}

		public Task<ErrorFileKey> GenerateImageErrorFilesAsync(int workspaceID, string runID, bool writeHeader, int keyFieldID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.RunID, runID);

			var result = _massImportManager.GenerateImageErrorFiles(GetBaseServiceContext(workspaceID), runID, workspaceID, writeHeader, keyFieldID).Map<ErrorFileKey>();
			return Task.FromResult(result);
		}

		public Task<bool> ImageRunHasErrorsAsync(int workspaceID, string runID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.RunID, runID);

			// there was a issue in image load logic, if there was no any correct image imported
			// then BulkImportImageAsync was not executed and runID was never set up, so there is no temp table
			if (string.IsNullOrEmpty(runID))
			{
				return Task.FromResult(false);
			}

			var result = _massImportManager.ImageRunHasErrors(GetBaseServiceContext(workspaceID), runID);
			return Task.FromResult(result);
		}

		public Task<ErrorFileKey> GenerateNonImageErrorFilesAsync(int workspaceID, string runID, int artifactTypeID, bool writeHeader, int keyFieldID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.RunID, runID);

			var result = _massImportManager.GenerateNonImageErrorFiles(GetBaseServiceContext(workspaceID), runID, artifactTypeID, writeHeader, keyFieldID).Map<ErrorFileKey>();
			return Task.FromResult(result);
		}

		public Task<bool> NativeRunHasErrorsAsync(int workspaceID, string runID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.RunID, runID);

			var result = _massImportManager.NativeRunHasErrors(GetBaseServiceContext(workspaceID), runID);
			return Task.FromResult(result);
		}

		public Task<object> DisposeTempTablesAsync(int workspaceID, string runID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.RunID, runID);

			var result = _massImportManager.DisposeRunTempTables(GetBaseServiceContext(workspaceID), runID);
			_batchResultCache.Cleanup(workspaceID, runID);
			return Task.FromResult(result);
		}

		public Task<bool> HasImportPermissionsAsync(int workspaceID, string correlationID)
		{
			var result = PermissionsHelper.HasAdminOperationPermission(GetBaseServiceContext(workspaceID), Permission.AllowDesktopClientImport);
			return Task.FromResult(result);
		}

		public Task<MassImportResults> GetBulkImportResultAsync(int workspaceID, string runID)
		{
			var existingResult = _batchResultCache.GetResult(workspaceID, runID);

			return Task.FromResult(existingResult);
		}

		public Task<List<ImportedDocumentInfo>> GetImportedNativesInfoAsync(int workspaceID, string runID, int keyFieldID)
		{
			var result =
				_massImportManager.GetImportedNativesInfo(GetBaseServiceContext(workspaceID), runID,  keyFieldID)
					.Map<List<ImportedDocumentInfo>>();

			return Task.FromResult(result);
		}

		public Task<List<ImportedDocumentInfo>> GetImportedImagesInfoAsync(int workspaceID, string runID, int keyFieldID)
		{
			var result =
				_massImportManager.GetImportedImagesInfo(GetBaseServiceContext(workspaceID), runID, keyFieldID)
					.Map<List<ImportedDocumentInfo>>();

			return Task.FromResult(result);
		}

		public Task<bool> NativeRunHasErrorsDoNotTruncateAsync(int workspaceID, string runID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.RunID, runID);

			var result = _massImportManager.NativeRunHasErrors(GetBaseServiceContext(workspaceID), runID, false);
			return Task.FromResult(result);
		}

		public Task<bool> ImageRunHasErrorsDoNotTruncateAsync(int workspaceID, string runID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.RunID, runID);

			// there was a issue in image load logic, if there was no any correct image imported
			// then BulkImportImageAsync was not executed and runID was never set up, so there is no temp table
			if (string.IsNullOrEmpty(runID))
			{
				return Task.FromResult(false);
			}

			var result = _massImportManager.ImageRunHasErrors(GetBaseServiceContext(workspaceID), runID, false);
			return Task.FromResult(result);
		}

		public Task<ErrorFileKey> GenerateNonImageErrorFilesDoNotTruncateAsync(int workspaceID, string runID, int artifactTypeID, bool writeHeader, int keyFieldID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.RunID, runID);

			var result = _massImportManager.GenerateNonImageErrorFiles(GetBaseServiceContext(workspaceID), runID, artifactTypeID, writeHeader, keyFieldID, false).Map<ErrorFileKey>();
			return Task.FromResult(result);
		}

		public Task<ErrorFileKey> GenerateImageErrorFilesDoNotTruncateAsync(int workspaceID, string runID, bool writeHeader, int keyFieldID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.RunID, runID);

			var result = _massImportManager.GenerateImageErrorFiles(GetBaseServiceContext(workspaceID), runID, workspaceID, writeHeader, keyFieldID, false).Map<ErrorFileKey>();
			return Task.FromResult(result);
		}
	}
}