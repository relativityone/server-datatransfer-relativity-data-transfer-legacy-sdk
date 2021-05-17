using System.Threading.Tasks;
using kCura.Utility;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	public class FolderService : BaseService, IFolderService
	{
		private readonly FolderManager _folderManager;

		public FolderService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) : base(methodRunner, serviceContextFactory)
		{
			_folderManager = new FolderManager();
		}

		public async Task<DataSetWrapper> RetrieveFolderAndDescendantsAsync(int workspaceID, int folderID, string correlationID)
		{
			return await ExecuteAsync(
				() => _folderManager.ExtenernalRetrieveFolderAndDescendants(GetBaseServiceContext(workspaceID), folderID),
				workspaceID, correlationID);
		}

		public async Task<Folder> ReadAsync(int workspaceID, int folderArtifactID, string correlationID)
		{
			return await ExecuteAsync(() =>
			{
				Core.DTO.Folder folder = FolderManager.Read(GetBaseServiceContext(workspaceID), folderArtifactID);
				folder.Name = XmlHelper.StripIllegalXmlCharacters(folder.Name);
				folder.TextIdentifier = XmlHelper.StripIllegalXmlCharacters(folder.TextIdentifier);
				return folder.Map<Folder>();
			}, workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<int> ReadIDAsync(int workspaceID, int parentArtifactID, string name, string correlationID)
		{
			return await ExecuteAsync(
				() => _folderManager.Read(GetBaseServiceContext(workspaceID), parentArtifactID, name),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<int> CreateAsync(int workspaceID, int parentArtifactID, string folderName, string correlationID)
		{
			return await ExecuteAsync(
				() => _folderManager.CheckFolderExistenceThenCreateWithoutDuplicates(GetBaseServiceContext(workspaceID), parentArtifactID, folderName),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<bool> ExistsAsync(int workspaceID, int folderArtifactID, string correlationID)
		{
			return await ExecuteAsync(
				() => _folderManager.Exists(GetBaseServiceContext(workspaceID), folderArtifactID),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveInitialChunkAsync(int workspaceID, string correlationID)
		{
			return await ExecuteAsync(
				() => _folderManager.RetrieveFolderChunk(GetBaseServiceContext(workspaceID)),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveNextChunkAsync(int workspaceID, int lastFolderID, string correlationID)
		{
			return await ExecuteAsync(
				() => _folderManager.RetrieveFolderChunk(GetBaseServiceContext(workspaceID), lastFolderID),
				workspaceID, correlationID).ConfigureAwait(false);
		}
	}
}