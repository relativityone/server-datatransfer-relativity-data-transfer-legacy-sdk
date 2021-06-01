using System;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core;
using Relativity.Core.Exception;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Runners;
using Permission = Relativity.Core.Permission;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
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


		public BulkImportService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) 
			: base(methodRunner, serviceContextFactory)
		{
			_massImportManager = new MassImportManager();
		}

		public Task<MassImportResults> BulkImportImageAsync(int workspaceID, SDK.ImportExport.V1.Models.ImageLoadInfo settings, bool inRepository, string correlationID)
		{
			return ExecuteAsync(() =>
			{
				IImportCoordinator coordinator = new ImageImportCoordinator(inRepository, settings);
				return BulkImport(workspaceID, coordinator);
			}, workspaceID, correlationID);
		}

		public Task<MassImportResults> BulkImportProductionImageAsync(int workspaceID, SDK.ImportExport.V1.Models.ImageLoadInfo settings, int productionArtifactID, bool inRepository, string correlationID)
		{
			return ExecuteAsync(() =>
			{
				IImportCoordinator coordinator = new ProductionImportCoordinator(inRepository, productionArtifactID, settings);
				return BulkImport(workspaceID, coordinator);
			}, workspaceID, correlationID);
		}

		public Task<MassImportResults> BulkImportNativeAsync(int workspaceID, SDK.ImportExport.V1.Models.NativeLoadInfo settings, bool inRepository, bool includeExtractedTextEncoding, string correlationID)
		{
			return ExecuteAsync(() =>
			{
				IImportCoordinator coordinator = new NativeImportCoordinator(inRepository, includeExtractedTextEncoding, settings);
				return BulkImport(workspaceID, coordinator);
			}, workspaceID, correlationID);
		}

		public Task<MassImportResults> BulkImportObjectsAsync(int workspaceID, SDK.ImportExport.V1.Models.ObjectLoadInfo settings, bool inRepository, string correlationID)
		{
			return ExecuteAsync(() =>
			{
				IImportCoordinator coordinator = new RdoImportCoordinator(inRepository, settings);
				return BulkImport(workspaceID, coordinator);
			}, workspaceID, correlationID);
		}

		private MassImportResults BulkImport(int workspaceID, IImportCoordinator coordinator)
		{
			BaseServiceContext serviceContext = GetBaseServiceContext(workspaceID);
			MassImportManager massImportManager = new MassImportManager();

			if (!massImportManager.HasImportPermission(serviceContext))
			{
				throw new InsufficientAccessControlListPermissions("Insufficient Permissions! Please ask your Relativity Administrator to allow you import permission.");
			}

			bool nonAdminsCanNoSnapshot = Config.AllowNoSnapshotImport;
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

				if (coordinator.ImportHasLinkedFiles() && Config.RestrictReferentialFileLinksOnImport)
				{
					return CreateResultWithException(FileSecurityWarning);
				}
			}

			MassImportManagerBase.MassImportResults results = coordinator.RunImport(serviceContext, massImportManager);
			if (coordinator.ArtifactTypeID == (int) ArtifactType.Document && Config.EnforceDocumentLimit)
			{
				results = massImportManager.PostImportDocumentLimitLogic(serviceContext, workspaceID, results);
			}

			return results.Map<MassImportResults>();
		}

		private static MassImportResults CreateResultWithException(string exceptionMessage)
		{
			SoapExceptionDetail soapExceptionDetail = new SoapExceptionDetail(new Exception(exceptionMessage));
			return new MassImportResults
			{
				ExceptionDetail = soapExceptionDetail.Map<SDK.ImportExport.V1.Models.SoapExceptionDetail>()
			};
		}

		public Task<ErrorFileKey> GenerateImageErrorFilesAsync(int workspaceID, string runID, bool writeHeader, int keyFieldID, string correlationID)
		{
			return ExecuteAsync(
				() => _massImportManager.GenerateImageErrorFiles(GetBaseServiceContext(workspaceID), runID, workspaceID, writeHeader, keyFieldID).Map<ErrorFileKey>(),
				workspaceID, correlationID);
		}

		public Task<bool> ImageRunHasErrorsAsync(int workspaceID, string runID, string correlationID)
		{
			return ExecuteAsync(
				() => _massImportManager.ImageRunHasErrors(GetBaseServiceContext(workspaceID), runID),
				workspaceID, correlationID);
		}

		public Task<ErrorFileKey> GenerateNonImageErrorFilesAsync(int workspaceID, string runID, int artifactTypeID, bool writeHeader, int keyFieldID, string correlationID)
		{
			return ExecuteAsync(
				() => _massImportManager.GenerateNonImageErrorFiles(GetBaseServiceContext(workspaceID), runID, artifactTypeID, writeHeader, keyFieldID).Map<ErrorFileKey>(),
				workspaceID, correlationID);
		}

		public Task<bool> NativeRunHasErrorsAsync(int workspaceID, string runID, string correlationID)
		{
			return ExecuteAsync(
				() => _massImportManager.NativeRunHasErrors(GetBaseServiceContext(workspaceID), runID),
				workspaceID, correlationID);
		}

		public Task<object> DisposeTempTablesAsync(int workspaceID, string runID, string correlationID)
		{
			return ExecuteAsync(
				() => _massImportManager.DisposeRunTempTables(GetBaseServiceContext(workspaceID), runID),
				workspaceID, correlationID);
		}

		public Task<bool> HasImportPermissionsAsync(int workspaceID, string correlationID)
		{
			return ExecuteAsync(
				() => PermissionsHelper.HasAdminOperationPermission(GetBaseServiceContext(workspaceID), Permission.AllowDesktopClientImport),
				workspaceID, correlationID);
		}
	}
}