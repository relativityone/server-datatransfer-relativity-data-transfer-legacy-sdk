using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("Code Manager replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("code")]
	public interface ICodeService : IDisposable
	{
		[HttpPost]
		[Route("")]
		Task<DataSetWrapper> RetrieveCodesAndTypesForCaseAsync(int workspaceID, string correlationID);

		/// <summary>
		/// return value is really an object (int or string, depending on errors)
		/// </summary>
		[HttpPost]
		[Route("CreateEncodedAsync")]
		Task<object> CreateEncodedAsync(int workspaceID, Code code, string correlationID);

		[HttpPost]
		[Route("ReadIDEncodedAsync")]
		Task<int> ReadIDEncodedAsync(int workspaceID, int parentArtifactID, int codeTypeID, [SensitiveData] string name, string correlationID);

		[HttpPost]
		[Route("GetAllForHierarchicalAsync")]
		Task<DataSetWrapper> GetAllForHierarchicalAsync(int workspaceID, int codeTypeID, string correlationID);

		[HttpPost]
		[Route("GetInitialChunkAsync")]
		Task<DataSetWrapper> GetInitialChunkAsync(int workspaceID, int codeTypeID, string correlationID);

		[HttpPost]
		[Route("GetLastChunkAsync")]
		Task<DataSetWrapper> GetLastChunkAsync(int workspaceID, int codeTypeID, int lastCodeID, string correlationID);

		[HttpPost]
		[Route("RetrieveCodeByNameAndTypeIDEncodedAsync")]
		Task<ChoiceInfo> RetrieveCodeByNameAndTypeIDEncodedAsync(int workspaceID, int codeTypeID, [SensitiveData] string name, string correlationID);

		[HttpPost]
		[Route("GetChoiceLimitForUIAsync")]
		Task<int> GetChoiceLimitForUIAsync(string correlationID);
	}
}