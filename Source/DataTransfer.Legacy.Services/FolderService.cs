using System.Diagnostics;
using System.Threading.Tasks;
using Castle.Core;
using kCura.Utility;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using TelemetryConstants = DataTransfer.Legacy.MassImport.RelEyeTelemetry.TelemetryConstants;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(DistributedTracingInterceptor))]
	public class FolderService : BaseService, IFolderService
	{
		private readonly FolderManager _folderManager;

		public FolderService(IServiceContextFactory serviceContextFactory)
			: base(serviceContextFactory)
		{
			_folderManager = new FolderManager();
		}

		public Task<DataSetWrapper> RetrieveFolderAndDescendantsAsync(int workspaceID, int folderID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, workspaceID);

			var result = _folderManager.ExtenernalRetrieveFolderAndDescendants(GetBaseServiceContext(workspaceID), folderID);
			return Task.FromResult(result != null ? new DataSetWrapper(result) : null);
		}

		public Task<Folder> ReadAsync(int workspaceID, int folderArtifactID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, workspaceID);

			var folder = FolderManager.Read(GetBaseServiceContext(workspaceID), folderArtifactID);
			folder.Name = XmlHelper.StripIllegalXmlCharacters(folder.Name);
			folder.TextIdentifier = XmlHelper.StripIllegalXmlCharacters(folder.TextIdentifier);
			var result = folder.Map<Folder>();
			return Task.FromResult(result);
		}

		public Task<int> ReadIDAsync(int workspaceID, int parentArtifactID, string name, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, workspaceID);

			var result = _folderManager.Read(GetBaseServiceContext(workspaceID), parentArtifactID, name);
			return Task.FromResult(result);
		}

		public Task<int> CreateAsync(int workspaceID, int parentArtifactID, string folderName, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, workspaceID);

			var result = _folderManager.CheckFolderExistenceThenCreateWithoutDuplicates(GetBaseServiceContext(workspaceID), parentArtifactID, folderName);
			return Task.FromResult(result);
		}

		public Task<bool> ExistsAsync(int workspaceID, int folderArtifactID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, workspaceID);

			var result = _folderManager.Exists(GetBaseServiceContext(workspaceID), folderArtifactID);
			return Task.FromResult(result);
		}

		public Task<DataSetWrapper> RetrieveInitialChunkAsync(int workspaceID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, workspaceID);

			var result = _folderManager.RetrieveFolderChunk(GetBaseServiceContext(workspaceID));
			return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
		}

		public Task<DataSetWrapper> RetrieveNextChunkAsync(int workspaceID, int lastFolderID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, workspaceID);

			var result = _folderManager.RetrieveFolderChunk(GetBaseServiceContext(workspaceID), lastFolderID);
			return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
		}
	}
}