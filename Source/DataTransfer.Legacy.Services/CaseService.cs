using System.Threading.Tasks;
using Castle.Core;
using kCura.Utility;
using Relativity.Core;
using Relativity.Core.DTO;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	public class CaseService : BaseService, ICaseService
	{
		private readonly CaseManager _caseManager;

		public CaseService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) 
			: base(methodRunner, serviceContextFactory)
		{
			_caseManager = new CaseManager();
		}

		public Task<SDK.ImportExport.V1.Models.CaseInfo> ReadAsync(int workspaceID, string correlationID)
		{
			return ExecuteAsync(() =>
			{
				BaseServiceContext instanceLevelContext = GetBaseServiceContext(AdminWorkspace);
				Case workspace = _caseManager.Read(instanceLevelContext, workspaceID);
				CaseInfo caseInfo = workspace.ToCaseInfo();

				string path = ResourceServerManager.Read(instanceLevelContext, workspace.DefaultFileLocationCodeArtifactID).URL;
				caseInfo.DocumentPath = path;
				caseInfo.Name = XmlHelper.StripIllegalXmlCharacters(caseInfo.Name);
				return caseInfo.Map<DataTransfer.Legacy.SDK.ImportExport.V1.Models.CaseInfo>();
			}, workspaceID, correlationID);
		}

		public Task<string[]> GetAllDocumentFolderPathsForCaseAsync(int workspaceID, string correlationID)
		{
			return ExecuteAsync(
				() => _caseManager.GetAllDocumentFolderPathsForCase(GetBaseServiceContext(AdminWorkspace), workspaceID),
				workspaceID, correlationID);
		}

		public Task<string[]> GetAllDocumentFolderPathsAsync(string correlationID)
		{
			return ExecuteAsync(
				() => _caseManager.GetAllDocumentFolderPaths(GetBaseServiceContext(AdminWorkspace)),
				null, correlationID);
		}

		public Task<DataSetWrapper> RetrieveAllEnabledAsync(string correlationID)
		{
			return ExecuteAsync(
				() => _caseManager.RetrieveAll(GetBaseServiceContext(AdminWorkspace), null, true),
				null, correlationID);
		}
	}
}