using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("Folder Manager replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("folder")]
	public interface IFolderService : IDisposable
	{
		[HttpPost]
		Task<DataSetWrapper> RetrieveFolderAndDescendantsAsync(int workspaceID, int folderID, string correlationID);

		[HttpPost]
		Task<Folder> ReadAsync(int workspaceID, int folderArtifactID, string correlationID);

		[HttpPost]
		Task<int> ReadIDAsync(int workspaceID, int parentArtifactID, [SensitiveData] string name, string correlationID);

		[HttpPost]
		Task<int> CreateAsync(int workspaceID, int parentArtifactID, [SensitiveData] string folderName, string correlationID);

		[HttpPost]
		Task<bool> ExistsAsync(int workspaceID, int folderArtifactID, string correlationID);

		[HttpPost]
		Task<DataSetWrapper> RetrieveInitialChunkAsync(int workspaceID, string correlationID);

		[HttpPost]
		Task<DataSetWrapper> RetrieveNextChunkAsync(int workspaceID, int lastFolderID, string correlationID);
	}
}