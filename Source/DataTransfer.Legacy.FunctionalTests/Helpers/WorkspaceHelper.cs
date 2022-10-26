using System;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Interfaces.Workspace;
using Relativity.Services.Interfaces.Workspace.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.Helpers
{
    public static class WorkspaceHelper
    {
        private static WorkspaceRequest _workspaceRequestCache;

        /// <summary>
        /// Creates new workspace based on <see cref="IntegrationTestParameters.WorkspaceTemplateName"/>.
        /// </summary>
        /// <param name="parameters">DTO with URL and credentials used to call REST API.</param>
        /// <returns>Task.</returns>
        public static async Task<TestWorkspace> CreateTestWorkspaceAsync(IntegrationTestParameters parameters)
        {
            WorkspaceRequest request = await GetWorkspaceRequestAsync(parameters).ConfigureAwait(false);

            using (var workspaceManager = ServiceHelper.GetServiceProxy<IWorkspaceManager>(parameters))
            {
                WorkspaceResponse response = await workspaceManager.CreateAsync(request).ConfigureAwait(false);
                return new TestWorkspace(parameters, response.ArtifactID, response.Name, response.DefaultFileRepository.Value.Name);
            }
        }

        public static async Task DeleteWorkspaceAsync(IntegrationTestParameters parameters, int workspaceArtifactId)
        {
            using (var workspaceManager = ServiceHelper.GetServiceProxy<IWorkspaceManager>(parameters))
            {
                await workspaceManager.DeleteAsync(workspaceArtifactId).ConfigureAwait(false);
            }
        }

        private static async Task<WorkspaceRequest> GetWorkspaceRequestAsync(IntegrationTestParameters parameters)
        {
            if (_workspaceRequestCache == null)
            {
                _workspaceRequestCache = await CreateWorkspaceRequestAsync(parameters).ConfigureAwait(false);
            }

            _workspaceRequestCache.Name = GetTestWorkspaceName();
            return _workspaceRequestCache;
        }

        private static string GetTestWorkspaceName()
        {
            return $"CompatibilityTests ({DateTime.Now:MM-dd HH.mm.ss.fff})";
        }

        private static async Task<WorkspaceRequest> CreateWorkspaceRequestAsync(IntegrationTestParameters parameters)
        {
            int templateWorkspaceArtifactId = await ReadWorkspaceArtifactId(parameters, parameters.WorkspaceTemplateName).ConfigureAwait(false);
            WorkspaceResponse templateWorkspace = await ReadWorkspaceAsync(parameters, templateWorkspaceArtifactId).ConfigureAwait(false);

            return new WorkspaceRequest(templateWorkspace)
            {
                Template = new Securable<ObjectIdentifier>(new ObjectIdentifier { ArtifactID = templateWorkspace.ArtifactID })
            };
        }

        private static async Task<WorkspaceResponse> ReadWorkspaceAsync(IntegrationTestParameters parameters, int workspaceArtifactId)
        {
            using (var workspaceManager = ServiceHelper.GetServiceProxy<IWorkspaceManager>(parameters))
            {
                return await workspaceManager.ReadAsync(workspaceArtifactId).ConfigureAwait(false);
            }
        }

        private static async Task<int> ReadWorkspaceArtifactId(IntegrationTestParameters parameters,
            string workspaceName)
        {
            const int workspaceArtifactTypeId = 8;

            using (var objectManager = ServiceHelper.GetServiceProxy<IObjectManager>(parameters))
            {
                QueryRequest queryRequest = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = workspaceArtifactTypeId },
                    Condition = $"'Name' == '{workspaceName}'",
                };

                QueryResult result = await objectManager.QueryAsync(-1, queryRequest, 0, 1).ConfigureAwait(false);
                return result.Objects.Single().ArtifactID;
            }
        }
    }
}