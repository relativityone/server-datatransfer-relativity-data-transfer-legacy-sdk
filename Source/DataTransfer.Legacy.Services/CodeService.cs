using System.Text;
using System.Threading.Tasks;
using System.Web;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	public class CodeService : BaseService, ICodeService
	{
		private readonly CodeManagerImplementation _codeManager;

		public CodeService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) : base(methodRunner, serviceContextFactory)
		{
			_codeManager = new CodeManagerImplementation();
		}

		public Task<DataSetWrapper> RetrieveCodesAndTypesForCaseAsync(int workspaceID, string correlationID)
		{
			return ExecuteAsync(
				() => _codeManager.ExternalRetrieveCodesAndTypesForCase(GetBaseServiceContext(workspaceID)),
				workspaceID, correlationID);
		}

		public Task<object> CreateEncodedAsync(int workspaceID, Code code, string correlationID)
		{
			return ExecuteAsync(() =>
			{
				code.Name = new string(Encoding.UTF8.GetChars(HttpServerUtility.UrlTokenDecode(code.Name)));
				return _codeManager.ExternalCreate(GetBaseServiceContext(workspaceID), code.Map<Core.DTO.Code>(), false);
			}, workspaceID, correlationID);
		}

		public Task<int> ReadIDEncodedAsync(int workspaceID, int parentArtifactID, int codeTypeID, string name, string correlationID)
		{
			return ExecuteAsync(() =>
			{
				name = new string(Encoding.UTF8.GetChars(HttpServerUtility.UrlTokenDecode(name)));
				return _codeManager.ReadID(GetBaseServiceContext(workspaceID), parentArtifactID, codeTypeID, name);
			}, workspaceID, correlationID);
		}

		public Task<DataSetWrapper> GetAllForHierarchicalAsync(int workspaceID, int codeTypeID, string correlationID)
		{
			return ExecuteAsync(
				() => Core.Query.Code.RetrieveHierarchicalByCodeTypeID(GetBaseServiceContext(workspaceID), codeTypeID),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> GetInitialChunkAsync(int workspaceID, int codeTypeID, string correlationID)
		{
			return ExecuteAsync(
				() => _codeManager.GetInitialCodeListChunk(GetBaseServiceContext(workspaceID), codeTypeID),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> GetLastChunkAsync(int workspaceID, int codeTypeID, int lastCodeID, string correlationID)
		{
			return ExecuteAsync(
				() => _codeManager.GetNextCodeListChunk(GetBaseServiceContext(workspaceID), codeTypeID, lastCodeID),
				workspaceID, correlationID);
		}

		public Task<DataTransfer.Legacy.SDK.ImportExport.V1.Models.ChoiceInfo> RetrieveCodeByNameAndTypeIDEncodedAsync(int workspaceID, int codeTypeID, string name, string correlationID)
		{
			return ExecuteAsync(() =>
			{
				name = new string(Encoding.UTF8.GetChars(HttpServerUtility.UrlTokenDecode(name)));
				return _codeManager.RetrieveCodeByNameAndTypeID(GetBaseServiceContext(workspaceID), codeTypeID, name).Map<DataTransfer.Legacy.SDK.ImportExport.V1.Models.ChoiceInfo>();
			}, workspaceID, correlationID);
		}

		public Task<int> GetChoiceLimitForUIAsync(string correlationID)
		{
			return ExecuteAsync(() => Config.ChoiceLimitForUI, null, correlationID);
		}
	}
}