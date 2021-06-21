using System.Threading.Tasks;
using Castle.Core;
using kCura.Utility;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
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
			var result = _folderManager.ExtenernalRetrieveFolderAndDescendants(GetBaseServiceContext(workspaceID), folderID);
			return Task.FromResult(new DataSetWrapper(result));
		}

		public Task<Folder> ReadAsync(int workspaceID, int folderArtifactID, string correlationID)
		{
			var folder = FolderManager.Read(GetBaseServiceContext(workspaceID), folderArtifactID);
			folder.Name = XmlHelper.StripIllegalXmlCharacters(folder.Name);
			folder.TextIdentifier = XmlHelper.StripIllegalXmlCharacters(folder.TextIdentifier);
			var result = folder.Map<Folder>();
			return Task.FromResult(result);
		}

		public Task<int> ReadIDAsync(int workspaceID, int parentArtifactID, string name, string correlationID)
		{
			var result = _folderManager.Read(GetBaseServiceContext(workspaceID), parentArtifactID, name);
			return Task.FromResult(result);
		}

		public Task<int> CreateAsync(int workspaceID, int parentArtifactID, string folderName, string correlationID)
		{
			var result = _folderManager.CheckFolderExistenceThenCreateWithoutDuplicates(GetBaseServiceContext(workspaceID), parentArtifactID, folderName);
			return Task.FromResult(result);
		}

		public Task<bool> ExistsAsync(int workspaceID, int folderArtifactID, string correlationID)
		{
			var result = _folderManager.Exists(GetBaseServiceContext(workspaceID), folderArtifactID);
			return Task.FromResult(result);
		}

		public Task<DataSetWrapper> RetrieveInitialChunkAsync(int workspaceID, string correlationID)
		{
			var result = _folderManager.RetrieveFolderChunk(GetBaseServiceContext(workspaceID));
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}

		public Task<DataSetWrapper> RetrieveNextChunkAsync(int workspaceID, int lastFolderID, string correlationID)
		{
			var result = _folderManager.RetrieveFolderChunk(GetBaseServiceContext(workspaceID), lastFolderID);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}
	}
}