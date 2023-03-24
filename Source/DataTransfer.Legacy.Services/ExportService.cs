using System;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.API;
using Relativity.Core;
using Relativity.Core.Exception;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Permission = Relativity.Core.Permission;

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
		private readonly IAPILog _logger;

		public ExportService(IServiceContextFactory serviceContextFactory, IAPILog logger) : base(serviceContextFactory)
		{
			_logger = logger;
		}

		public Task<InitializationResults> InitializeSearchExportAsync(int workspaceID, int searchArtifactID, int[] avfIDs, int startAtRecord, string correlationID)
		{
			var result = InitializeExport(workspaceID, (int)ArtifactType.Document,
						e => e.InitializeSavedSearchExport(searchArtifactID, DynamicallyLoadedDllPaths, avfIDs,
							startAtRecord).Map<InitializationResults>());
			return Task.FromResult(result);
		}

		public Task<InitializationResults> InitializeFolderExportAsync(int workspaceID, int viewArtifactID,
			int parentArtifactID, bool includeSubFolders, int[] avfIDs, int startAtRecord, int artifactTypeID,
			string correlationID)
		{
			var result = InitializeExport(workspaceID, artifactTypeID,
				e => e.InitializeFolderExport(viewArtifactID, parentArtifactID, includeSubFolders,
					DynamicallyLoadedDllPaths, avfIDs, startAtRecord).Map<InitializationResults>());
			return Task.FromResult(result);
		}

		public Task<InitializationResults> InitializeProductionExportAsync(int workspaceID, int productionArtifactID,
			int[] avfIds, int startAtRecord, string correlationID)
		{
			var result = InitializeExport(workspaceID, (int)ArtifactType.Document,
				e => e.InitializeProductionExport(productionArtifactID, DynamicallyLoadedDllPaths, avfIds,
					startAtRecord).Map<InitializationResults>());
			return Task.FromResult(result);
		}

		public Task<ExportDataWrapper> RetrieveResultsBlockForProductionStartingFromIndexAsync(int workspaceID, Guid runID, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter,
			char nestedValueDelimiter,
			int[] textPrecedenceAvfIds, int productionId, int index, string correlationID)
		{
			var result = RetrieveResults(workspaceID, runID, artifactTypeID, avfIds, chunkSize, displayMulticodesAsNested,
					multiValueDelimiter, nestedValueDelimiter, textPrecedenceAvfIds, productionId, index);
			return Task.FromResult(result);
		}

		public Task<ExportDataWrapper> RetrieveResultsBlockStartingFromIndexAsync(int workspaceID, Guid runID, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter,
			int[] textPrecedenceAvfIds, int index, string correlationID)
		{
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

			return WrapResults(result);
		}

		private ExportDataWrapper WrapResults(object[] result)
		{
			// REL-797147, System.OutOfMemoryException: Array dimensions exceeded supported range
			const int maximumSerializedDataLength = 456176861; // 1024 rows, 1024 columns, 384 characters per each element

			var dataWrapper = new ExportDataWrapper(result);
			while (dataWrapper.SerializedDataLength > maximumSerializedDataLength && result.Length > 1)
			{
				int newResultsLength = result.Length / 2;
				_logger.LogWarning(
					"Length of ExportDataWrapper is too large: '{length}'. Reducing size of result set from {length} to {newLength} records.",
					dataWrapper.SerializedDataLength,
					result.Length,
					newResultsLength);

				dataWrapper = null;
				result = result.Take(newResultsLength).ToArray();
				dataWrapper = new ExportDataWrapper(result);
			}

			return dataWrapper;
		}

		public Task<bool> HasExportPermissionsAsync(int workspaceID, string correlationID)
		{
			var result = PermissionsHelper.HasAdminOperationPermission(GetBaseServiceContext(workspaceID), Permission.AllowDesktopClientExport);
			return Task.FromResult(result);
		}
	}
}