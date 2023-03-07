using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.ExternalServices
{
	public interface IFileRepositoryExternalService
	{
		Task<string[]> GetAllDocumentFolderPathsForCase(int workspaceID);
	}
}
