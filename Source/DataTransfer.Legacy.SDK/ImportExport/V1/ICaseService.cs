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
		[Route("ReadAsync")]
		Task<CaseInfo> ReadAsync(int workspaceID, string correlationID);

		[HttpPost]
		[Route("GetAllDocumentFolderPathsForCaseAsync")]
		Task<string[]> GetAllDocumentFolderPathsForCaseAsync(int workspaceID, string correlationID);

		[HttpPost]
		[Route("GetAllDocumentFolderPathsAsync")]
		Task<string[]> GetAllDocumentFolderPathsAsync(string correlationID);

		[HttpPost]
		[Route("RetrieveAllEnabledAsync")]
		Task<DataSetWrapper> RetrieveAllEnabledAsync(string correlationID);
	}
}