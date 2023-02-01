using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core;
using Relativity.Core.Exception;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Permission = Relativity.Core.Permission;
using TelemetryConstants = DataTransfer.Legacy.MassImport.RelEyeTelemetry.TelemetryConstants;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(DistributedTracingInterceptor))]

	public class ExportService : BaseService, IExportService
	{
		private static readonly string[] DynamicallyLoadedDllPaths = { Config.DynamicallyLoadedStandardSearchDLLs, Config.DynamicallyLoadedClientSearchDLLs };

		public ExportService(IServiceContextFactory serviceContextFactory) : base(serviceContextFactory) { }

		public Task<InitializationResults> InitializeSearchExportAsync(int workspaceID, int searchArtifactID, int[] avfIDs, int startAtRecord, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, workspaceID);

			var result = InitializeExport(workspaceID, (int)ArtifactType.Document,
						e => e.InitializeSavedSearchExport(searchArtifactID, DynamicallyLoadedDllPaths, avfIDs,
							startAtRecord).Map<InitializationResults>());
			return Task.FromResult(result);
		}

		public Task<InitializationResults> InitializeFolderExportAsync(int workspaceID, int viewArtifactID,
			int parentArtifactID, bool includeSubFolders, int[] avfIDs, int startAtRecord, int artifactTypeID,
			string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, workspaceID);

			var result = InitializeExport(workspaceID, artifactTypeID,
				e => e.InitializeFolderExport(viewArtifactID, parentArtifactID, includeSubFolders,
					DynamicallyLoadedDllPaths, avfIDs, startAtRecord).Map<InitializationResults>());
			return Task.FromResult(result);
		}

		public Task<InitializationResults> InitializeProductionExportAsync(int workspaceID, int productionArtifactID,
			int[] avfIds, int startAtRecord, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, workspaceID);

			var result = InitializeExport(workspaceID, (int)ArtifactType.Document,
				e => e.InitializeProductionExport(productionArtifactID, DynamicallyLoadedDllPaths, avfIds,
					startAtRecord).Map<InitializationResults>());
			return Task.FromResult(result);
		}

		public Task<ExportDataWrapper> RetrieveResultsBlockForProductionStartingFromIndexAsync(int workspaceID, Guid runID, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter,
			char nestedValueDelimiter,
			int[] textPrecedenceAvfIds, int productionId, int index, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, workspaceID);

			var result = RetrieveResults(workspaceID, runID, artifactTypeID, avfIds, chunkSize, displayMulticodesAsNested,
					multiValueDelimiter, nestedValueDelimiter, textPrecedenceAvfIds, productionId, index);
			return Task.FromResult(result);
		}

		public Task<ExportDataWrapper> RetrieveResultsBlockStartingFromIndexAsync(int workspaceID, Guid runID, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter,
			int[] textPrecedenceAvfIds, int index, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, workspaceID);

			var result = RetrieveResults(workspaceID, runID, artifactTypeID, avfIds, chunkSize, displayMulticodesAsNested,
					multiValueDelimiter, nestedValueDelimiter, textPrecedenceAvfIds, null, index);
			return Task.FromResult(result);
		}

		private InitializationResults InitializeExport(int workspaceID, int artifactTypeID, Func<Core.Export, InitializationResults> initialization)
		{
			var export = new Core.Export(GetBaseServiceContext(workspaceID), GetUserAclMatrix(workspaceID), artifactTypeID);
			if (!export.HasExportPermissions())
			{
				throw new InsufficientAccessControlListPermissions("Insufficient Permissions! Please ask your Relativity Administrator to allow you export permission.");
			}

			return initialization(export);
		}

		private ExportDataWrapper RetrieveResults(int workspaceID, Guid runID, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter, int[] textPrecedenceAvfIds,
			int? productionId,
			int index)
		{
			var export = new Core.Export(GetBaseServiceContext(workspaceID), GetUserAclMatrix(workspaceID), artifactTypeID, textPrecedenceAvfIds);
			if (!export.HasExportPermissions())
			{
				throw new InsufficientAccessControlListPermissions("Insufficient Permissions! Please ask your Relativity Administrator to allow you export permission.");
			}

			object[] result;
			if (productionId.HasValue)
			{
				result = export.RetrieveResultsBlockForProductionStartingFromIndex(GetBaseServiceContext(workspaceID), runID, avfIds, chunkSize, displayMulticodesAsNested, multiValueDelimiter, nestedValueDelimiter, productionId.Value, index);
			}
			else
			{
				result = export.RetrieveResultsBlockStartingFromIndex(GetBaseServiceContext(workspaceID), runID, avfIds, chunkSize, displayMulticodesAsNested, multiValueDelimiter, nestedValueDelimiter, index);
			}

			return new ExportDataWrapper(result);
		}

		public Task<bool> HasExportPermissionsAsync(int workspaceID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, workspaceID);

			var result = PermissionsHelper.HasAdminOperationPermission(GetBaseServiceContext(workspaceID), Permission.AllowDesktopClientExport);
			return Task.FromResult(result);
		}
	}
}