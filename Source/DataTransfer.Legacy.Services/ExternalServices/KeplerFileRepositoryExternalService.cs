using Polly;
using Relativity.DataTransfer.Legacy.Services.ExternalServices.RetryPolicies;
using Relativity.Services.Interfaces.ResourceServer;
using Relativity.Services.Interfaces.Workspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.ResourceServer.Models;

namespace Relativity.DataTransfer.Legacy.Services.ExternalServices
{
	internal class KeplerFileRepositoryExternalService : IFileRepositoryExternalService
	{
		private readonly IWorkspaceManager _workspaceManager;
		private readonly IFileRepositoryServerManager _fileRepositoryServerManager;
		private readonly Lazy<IAsyncPolicy> _retryPolicyLazy;

		public KeplerFileRepositoryExternalService(
			IWorkspaceManager workspaceManager,
			IFileRepositoryServerManager fileRepositoryServerManager,
			IKeplerRetryPolicyFactory retryPolicyFactory)
		{
			_workspaceManager = workspaceManager;
			_fileRepositoryServerManager = fileRepositoryServerManager;
			_retryPolicyLazy = new Lazy<IAsyncPolicy>(retryPolicyFactory.CreateRetryPolicy);
		}

		public async Task<string[]> GetAllDocumentFolderPathsForCase(int workspaceID)
		{
			var workspace = await _retryPolicyLazy.Value.ExecuteAsync(() =>
				_workspaceManager.ReadAsync(workspaceID));

			var fileRepositories = await _retryPolicyLazy.Value.ExecuteAsync(() =>
				_workspaceManager.GetEligibleFileRepositoriesAsync(workspace.ResourcePool.Value.ArtifactID));

			var tasks = new List<Task<FileRepositoryServerResponse>>();
			foreach (var fileRepositoryId in fileRepositories.Select(x => x.ArtifactID))
			{
				var task = _retryPolicyLazy.Value.ExecuteAsync(() => _fileRepositoryServerManager.ReadAsync(fileRepositoryId));
				tasks.Add(task);
			}

			var responses = await Task.WhenAll(tasks);

			return responses.Select(x => x.UncPath).ToArray();
		}
	}
}
