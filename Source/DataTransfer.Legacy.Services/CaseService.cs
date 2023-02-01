using System.Threading.Tasks;
using Castle.Core;
using kCura.Utility;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(DistributedTracingInterceptor))]
	public class CaseService : BaseService, ICaseService
	{
		private readonly CaseManager _caseManager;

		public CaseService(IServiceContextFactory serviceContextFactory)
			: base(serviceContextFactory)
		{
			_caseManager = new CaseManager();
		}

		public Task<SDK.ImportExport.V1.Models.CaseInfo> ReadAsync(int workspaceID, string correlationID)
		{
			var instanceLevelContext = GetBaseServiceContext(AdminWorkspace);
			var workspace = _caseManager.Read(instanceLevelContext, workspaceID);
			var caseInfo = workspace.ToCaseInfo();

			var path = ResourceServerManager.Read(instanceLevelContext, workspace.DefaultFileLocationCodeArtifactID).URL;
			caseInfo.DocumentPath = path;
			caseInfo.Name = XmlHelper.StripIllegalXmlCharacters(caseInfo.Name);
			var result = caseInfo.Map<SDK.ImportExport.V1.Models.CaseInfo>();
			return Task.FromResult(result);
		}

		public Task<string[]> GetAllDocumentFolderPathsForCaseAsync(int workspaceID, string correlationID)
		{
			var result = _caseManager.GetAllDocumentFolderPathsForCase(GetBaseServiceContext(AdminWorkspace), workspaceID);
			return Task.FromResult(result);
		}

		public Task<string[]> GetAllDocumentFolderPathsAsync(string correlationID)
		{
			var result = _caseManager.GetAllDocumentFolderPaths(GetBaseServiceContext(AdminWorkspace));
			return Task.FromResult(result);
		}

		public Task<DataSetWrapper> RetrieveAllEnabledAsync(string correlationID)
		{
			var result = _caseManager.RetrieveAll(GetBaseServiceContext(AdminWorkspace), null, true);
			return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
		}
	}
}