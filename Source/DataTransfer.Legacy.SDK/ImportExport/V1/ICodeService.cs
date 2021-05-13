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
		Task<DataSetWrapper> RetrieveCodesAndTypesForCaseAsync(int workspaceID, string correlationID);

		/// <summary>
		/// return value is really an object (int or string, depending on errors)
		/// </summary>
		[HttpPost]
		Task<object> CreateEncodedAsync(int workspaceID, Code code, string correlationID);

		[HttpPost]
		Task<int> ReadIDEncodedAsync(int workspaceID, int parentArtifactID, int codeTypeID, string name, string correlationID);

		[HttpPost]
		Task<DataSetWrapper> GetAllForHierarchicalAsync(int workspaceID, int codeTypeID, string correlationID);

		[HttpPost]
		Task<DataSetWrapper> GetInitialChunkAsync(int workspaceID, int codeTypeID, string correlationID);

		[HttpPost]
		Task<DataSetWrapper> GetLastChunkAsync(int workspaceID, int codeTypeID, int lastCodeID, string correlationID);

		[HttpPost]
		Task<ChoiceInfo> RetrieveCodeByNameAndTypeIDEncodedAsync(int workspaceID, int codeTypeID, string name, string correlationID);

		[HttpPost]
		Task<int> GetChoiceLimitForUIAsync(string correlationID);
	}
}