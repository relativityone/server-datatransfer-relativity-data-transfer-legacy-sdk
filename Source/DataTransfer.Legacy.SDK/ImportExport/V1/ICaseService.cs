using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("Case Manager replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("case")]
	public interface ICaseService : IDisposable
	{
		[HttpPost]
		Task<CaseInfo> ReadAsync(int workspaceID, string correlationID);

		[HttpPost]
		Task<string[]> GetAllDocumentFolderPathsForCaseAsync(int workspaceID, string correlationID);

		[HttpPost]
		Task<string[]> GetAllDocumentFolderPathsAsync(string correlationID);

		[HttpPost]
		Task<DataSetWrapper> RetrieveAllEnabledAsync(string correlationID);
	}
}