using System.Threading.Tasks;
using Castle.Core;
using kCura.Utility;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Runners;

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

		public FolderService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) 
			: base(methodRunner, serviceContextFactory)
		{
			_folderManager = new FolderManager();
		}

		public Task<DataSetWrapper> RetrieveFolderAndDescendantsAsync(int workspaceID, int folderID, string correlationID)
		{
			return ExecuteAsync(
				() => _folderManager.ExtenernalRetrieveFolderAndDescendants(GetBaseServiceContext(workspaceID), folderID),
				workspaceID, correlationID);
		}

		public Task<Folder> ReadAsync(int workspaceID, int folderArtifactID, string correlationID)
		{
			return ExecuteAsync(() =>
			{
				Core.DTO.Folder folder = FolderManager.Read(GetBaseServiceContext(workspaceID), folderArtifactID);
				folder.Name = XmlHelper.StripIllegalXmlCharacters(folder.Name);
				folder.TextIdentifier = XmlHelper.StripIllegalXmlCharacters(folder.TextIdentifier);
				return folder.Map<Folder>();
			}, workspaceID, correlationID);
		}

		public Task<int> ReadIDAsync(int workspaceID, int parentArtifactID, string name, string correlationID)
		{
			return ExecuteAsync(
				() => _folderManager.Read(GetBaseServiceContext(workspaceID), parentArtifactID, name),
				workspaceID, correlationID);
		}

		public Task<int> CreateAsync(int workspaceID, int parentArtifactID, string folderName, string correlationID)
		{
			return ExecuteAsync(
				() => _folderManager.CheckFolderExistenceThenCreateWithoutDuplicates(GetBaseServiceContext(workspaceID), parentArtifactID, folderName),
				workspaceID, correlationID);
		}

		public Task<bool> ExistsAsync(int workspaceID, int folderArtifactID, string correlationID)
		{
			return ExecuteAsync(
				() => _folderManager.Exists(GetBaseServiceContext(workspaceID), folderArtifactID),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveInitialChunkAsync(int workspaceID, string correlationID)
		{
			return ExecuteAsync(
				() => _folderManager.RetrieveFolderChunk(GetBaseServiceContext(workspaceID)),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveNextChunkAsync(int workspaceID, int lastFolderID, string correlationID)
		{
			return ExecuteAsync(
				() => _folderManager.RetrieveFolderChunk(GetBaseServiceContext(workspaceID), lastFolderID),
				workspaceID, correlationID);
		}
	}
}