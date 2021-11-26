using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Folder;

namespace MassImport.NUnit.Integration.Helpers
{
	internal static class FolderHelper
	{
		public static async Task<int> ReadRootFolderIdAsync(IntegrationTestParameters parameters, TestWorkspace workspace)
		{
			using (IFolderManager folderManager = ServiceHelper.GetServiceProxy<IFolderManager>(parameters))
			{
				var folder = await folderManager.GetWorkspaceRootAsync(workspace.WorkspaceId).ConfigureAwait(false);
				return folder.ArtifactID;
			}
		}

		public static async Task<List<int>> CreateFoldersAsync(IntegrationTestParameters parameters, TestWorkspace workspace, IEnumerable<string> folders, int rootFolderArtifactId)
		{
			using (IFolderManager folderManager = ServiceHelper.GetServiceProxy<IFolderManager>(parameters))
			{
				var folderRef = new FolderRef(rootFolderArtifactId);

				var taskResults = await Task.WhenAll(
					folders.Select(
						folderName => folderManager?.CreateSingleAsync(
							workspace.WorkspaceId,
							new Folder() 
								{
									Name = folderName, 
									ParentFolder = folderRef
								}))
						.ToArray())
					.ConfigureAwait(false);

				return taskResults.ToList();
			}
		}

		public static async Task DeleteUnusedFoldersAsync(IntegrationTestParameters parameters, TestWorkspace workspace)
		{
			using (IFolderManager folderManager = ServiceHelper.GetServiceProxy<IFolderManager>(parameters))
			{
				await folderManager.DeleteUnusedFoldersAsync(workspace.WorkspaceId).ConfigureAwait(false);
			}
		}
	}
}
