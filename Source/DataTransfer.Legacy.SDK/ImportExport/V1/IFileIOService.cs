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
		Task<IoResponse> BeginFillAsync(int workspaceID, [SensitiveData] byte[] b, string documentDirectory, string fileName, string correlationID);

		[HttpPost]
		Task<IoResponse> FileFillAsync(int workspaceID, string documentDirectory, string fileName, [SensitiveData] byte[] b, string correlationID);

		[HttpPost]
		Task RemoveFillAsync(int workspaceID, string documentDirectory, string fileName, string correlationID);

		[HttpPost]
		Task RemoveTempFileAsync(int workspaceID, string fileName, string correlationID);

		[HttpPost]
		Task<string[][]> GetDefaultRepositorySpaceReportAsync(int workspaceID, string correlationID);

		[HttpPost]
		Task<string[][]> GetBcpShareSpaceReportAsync(int workspaceID, string correlationID);

		[HttpPost]
		Task<string> GetBcpSharePathAsync(int workspaceID, string correlationID);

		[HttpPost]
		Task<bool> ValidateBcpShareAsync(int workspaceID, string correlationID);

		[HttpPost]
		Task<int> RepositoryVolumeMaxAsync(string correlationID);
	}
}