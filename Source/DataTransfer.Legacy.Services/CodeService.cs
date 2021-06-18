using System.Text;
using System.Threading.Tasks;
using System.Web;
using Castle.Core;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	public class CodeService : BaseService, ICodeService
	{
		private readonly CodeManagerImplementation _codeManager;

		public CodeService(IServiceContextFactory serviceContextFactory)
			: base(serviceContextFactory)
		{
			_codeManager = new CodeManagerImplementation();
		}

		public Task<DataSetWrapper> RetrieveCodesAndTypesForCaseAsync(int workspaceID, string correlationID)
		{
			var result = _codeManager.ExternalRetrieveCodesAndTypesForCase(GetBaseServiceContext(workspaceID));
			return Task.FromResult(new DataSetWrapper(result));
		}

		public Task<object> CreateEncodedAsync(int workspaceID, Code code, string correlationID)
		{
			code.Name = new string(Encoding.UTF8.GetChars(HttpServerUtility.UrlTokenDecode(code.Name)));
			var result = _codeManager.ExternalCreate(GetBaseServiceContext(workspaceID), code.Map<Core.DTO.Code>(), false);
			return Task.FromResult(result);
		}

		public Task<int> ReadIDEncodedAsync(int workspaceID, int parentArtifactID, int codeTypeID, string name,
			string correlationID)
		{
			name = new string(Encoding.UTF8.GetChars(HttpServerUtility.UrlTokenDecode(name)));
			var result = _codeManager.ReadID(GetBaseServiceContext(workspaceID), parentArtifactID, codeTypeID, name);
			return Task.FromResult(result);
		}

		public Task<DataSetWrapper> GetAllForHierarchicalAsync(int workspaceID, int codeTypeID, string correlationID)
		{
			var result = Core.Query.Code.RetrieveHierarchicalByCodeTypeID(GetBaseServiceContext(workspaceID), codeTypeID);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}

		public Task<DataSetWrapper> GetInitialChunkAsync(int workspaceID, int codeTypeID, string correlationID)
		{
			var result = _codeManager.GetInitialCodeListChunk(GetBaseServiceContext(workspaceID), codeTypeID);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}

		public Task<DataSetWrapper> GetLastChunkAsync(int workspaceID, int codeTypeID, int lastCodeID,
			string correlationID)
		{
			var result = _codeManager.GetNextCodeListChunk(GetBaseServiceContext(workspaceID), codeTypeID, lastCodeID);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}

		public Task<SDK.ImportExport.V1.Models.ChoiceInfo> RetrieveCodeByNameAndTypeIDEncodedAsync(
			int workspaceID, int codeTypeID, string name, string correlationID)
		{
			name = new string(Encoding.UTF8.GetChars(HttpServerUtility.UrlTokenDecode(name)));
			var result = _codeManager.RetrieveCodeByNameAndTypeID(GetBaseServiceContext(workspaceID), codeTypeID, name)
				.Map<SDK.ImportExport.V1.Models.ChoiceInfo>();
			return Task.FromResult(result);
		}

		public Task<int> GetChoiceLimitForUIAsync(string correlationID)
		{
			var result = Config.ChoiceLimitForUI;
			return Task.FromResult(result);
		}
	}
}