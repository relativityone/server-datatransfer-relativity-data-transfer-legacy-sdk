using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("FileIO replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("file-io")]
	public interface IFileIOService : IDisposable
	{
		[HttpPost]
		[Route("BeginFillAsync")]
		Task<IoResponse> BeginFillAsync(int workspaceID, [SensitiveData] byte[] b, string documentDirectory, string fileName, string correlationID);

		[HttpPost]
		[Route("FileFillAsync")]
		Task<IoResponse> FileFillAsync(int workspaceID, string documentDirectory, string fileName, [SensitiveData] byte[] b, string correlationID);

		[HttpPost]
		[Route("RemoveFillAsync")]
		Task RemoveFillAsync(int workspaceID, string documentDirectory, string fileName, string correlationID);

		[HttpPost]
		[Route("RemoveTempFileAsync")]
		Task RemoveTempFileAsync(int workspaceID, string fileName, string correlationID);

		[HttpPost]
		[Route("GetDefaultRepositorySpaceReportAsync")]
		Task<string[][]> GetDefaultRepositorySpaceReportAsync(int workspaceID, string correlationID);

		[HttpPost]
		[Route("GetBcpShareSpaceReportAsync")]
		Task<string[][]> GetBcpShareSpaceReportAsync(int workspaceID, string correlationID);

		[HttpPost]
		[Route("GetBcpSharePathAsync")]
		Task<string> GetBcpSharePathAsync(int workspaceID, string correlationID);

		[HttpPost]
		[Route("ValidateBcpShareAsync")]
		Task<bool> ValidateBcpShareAsync(int workspaceID, string correlationID);

		[HttpPost]
		[Route("RepositoryVolumeMaxAsync")]
		Task<int> RepositoryVolumeMaxAsync(string correlationID);
	}
}