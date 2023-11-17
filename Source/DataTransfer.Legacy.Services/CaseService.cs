using System;
using System.Threading.Tasks;
using Castle.Core;
using kCura.Utility;
using Relativity.API;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.ExternalServices;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Toggles;
using Relativity.Toggles;

namespace Relativity.DataTransfer.Legacy.Services
{
	using Relativity.Services.Exceptions;

	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(DistributedTracingInterceptor))]
	public class CaseService : BaseService, ICaseService
	{
		private readonly CaseManager _caseManager;

		private readonly IAPILog _logger;
		private readonly IFileRepositoryExternalService _fileRepositoryExternalService;
		private readonly IToggleProvider _toggleProvider;

		public CaseService(IServiceContextFactory serviceContextFactory, IAPILog logger, IFileRepositoryExternalService fileRepositoryExternalService, IToggleProvider toggleProvider)
			: base(serviceContextFactory)
		{
			_caseManager = new CaseManager();
			_logger = logger;
			_fileRepositoryExternalService = fileRepositoryExternalService;
			_toggleProvider = toggleProvider;
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

		public async Task<string[]> GetAllDocumentFolderPathsForCaseAsync(int workspaceID, string correlationID)
		{
			return await _fileRepositoryExternalService.GetAllDocumentFolderPathsForCase(workspaceID);
		}

		public async Task<string[]> GetAllDocumentFolderPathsAsync(string correlationID)
		{
			var isRdcDisabled = await _toggleProvider.IsEnabledAsync<DisableRdcAndImportApiToggle>();

			if (isRdcDisabled)
			{
				_logger.LogWarning("RDC and Import API have been have been deprecated in this RelativityOne instance.");

				throw new NotFoundException(Constants.ErrorMessages.RdcDeprecatedDisplayMessage);

			}
			var result = _caseManager.GetAllDocumentFolderPaths(GetBaseServiceContext(AdminWorkspace));
			return result;
		}

		public Task<DataSetWrapper> RetrieveAllEnabledAsync(string correlationID)
		{
			var result = _caseManager.RetrieveAll(GetBaseServiceContext(AdminWorkspace), null, true);
			return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
		}
	}
}