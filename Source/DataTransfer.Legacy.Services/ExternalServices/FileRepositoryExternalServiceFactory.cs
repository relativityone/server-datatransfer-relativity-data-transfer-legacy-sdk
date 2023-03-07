using Relativity.DataTransfer.Legacy.Services.ExternalServices.RetryPolicies;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.Services.Interfaces.ResourceServer;
using Relativity.Services.Interfaces.Workspace;
using Relativity.API;
using Relativity.Toggles;
using Relativity.DataTransfer.Legacy.Services.Toggles;

namespace Relativity.DataTransfer.Legacy.Services.ExternalServices
{
	internal class FileRepositoryExternalServiceFactory : IFileRepositoryExternalServiceFactory
	{

		private readonly IServiceContextFactory _serviceContextFactory;
		private readonly IWorkspaceManager _workspaceManager;
		private readonly IFileRepositoryServerManager _fileRepositoryServerManager;
		private readonly IKeplerRetryPolicyFactory _retryPolicyFactory;
		private readonly IToggleProvider _toggleProvider;

		public FileRepositoryExternalServiceFactory(
			IServiceContextFactory serviceContextFactory,
			IWorkspaceManager workspaceManager,
			IFileRepositoryServerManager fileRepositoryServerManager,
			IKeplerRetryPolicyFactory retryPolicyFactory,
			IToggleProvider toggleProvider,
			IAPILog logger)
		{
			_serviceContextFactory = serviceContextFactory;
			_workspaceManager = workspaceManager;
			_fileRepositoryServerManager = fileRepositoryServerManager;
			_retryPolicyFactory = retryPolicyFactory;
			_toggleProvider = toggleProvider;
		}

		public IFileRepositoryExternalService Create()
		{
			if (_toggleProvider.IsEnabled<DisableKeplerFileRepositoryQueryToggle>())
			{
				return new LegacyFileRepositoryExternalService(_serviceContextFactory);
			}
			else
			{
				return new KeplerFileRepositoryExternalService(_workspaceManager, _fileRepositoryServerManager, _retryPolicyFactory);
			}
		}
	}
}
