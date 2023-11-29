using System.Threading.Tasks;
using Relativity.Services.Folder;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Kepler;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.Helpers
{
	internal static class FolderHelper
	{
		public static async Task<int> ReadRootFolderIdAsync(int workspaceId)
		{
			IKeplerServiceFactory serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
			using (IFolderManager folderManager = serviceFactory.GetServiceProxy<IFolderManager>())
			{
				var folder = await folderManager.GetWorkspaceRootAsync(workspaceId).ConfigureAwait(false);
				return folder.ArtifactID;
			}
		}

	}
}
