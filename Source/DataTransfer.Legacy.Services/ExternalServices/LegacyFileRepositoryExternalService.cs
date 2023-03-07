using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.ExternalServices
{
	internal class LegacyFileRepositoryExternalService : IFileRepositoryExternalService
	{
		private readonly CaseManager _caseManager;
		private readonly IServiceContextFactory _serviceContextFactory;
		private static int AdminWorkspace = -1;

		public LegacyFileRepositoryExternalService(IServiceContextFactory serviceContextFactory)
		{
			_caseManager = new CaseManager();
			_serviceContextFactory = serviceContextFactory;
		}

		public Task<string[]> GetAllDocumentFolderPathsForCase(int workspaceID)
		{
			var baseServiceContext = _serviceContextFactory.GetBaseServiceContext(AdminWorkspace);
			var result = _caseManager.GetAllDocumentFolderPathsForCase(baseServiceContext, workspaceID);
			return Task.FromResult(result);
		}
	}
}
