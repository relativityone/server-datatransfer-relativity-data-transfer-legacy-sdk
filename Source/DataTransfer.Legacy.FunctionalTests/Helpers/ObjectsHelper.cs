using System.Linq;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Kepler;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.Helpers
{
	internal class ObjectsHelper
	{
		/// <summary>
		/// Deletes all RDOs of a given type from a test workspace in batches.
		/// </summary>
		/// <param name="workspace">Workspace</param>
		/// <param name="artifactTypeID">Type of objects to delete.</param>
		/// <returns><see cref="Task"/> which completes when all RDOs are deleted.</returns>
		internal static async Task DeleteAllObjectsByTypeAsync(Testing.Framework.Models.Workspace workspace, int artifactTypeID)
		{
			const int DeleteBatchSize = 250;

			IKeplerServiceFactory serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
			// Deleting objects in a small batches is more stable than deleting all objects of a given type at one go.
			// Please see https://jira.kcura.com/browse/REL-496822 and https://jira.kcura.com/browse/REL-478746 for details.
			using (IObjectManager objectManager = serviceFactory.GetServiceProxy<IObjectManager>())
			{
				var queryAllObjectsRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						ArtifactTypeID = artifactTypeID,
					},
				};

				while (true)
				{
					var existingArtifacts = await objectManager
						.QuerySlimAsync(workspace.ArtifactID, queryAllObjectsRequest, start: 0, length: DeleteBatchSize)
						.ConfigureAwait(false);
					var objectRefs = existingArtifacts.Objects
						.Select(x => x.ArtifactID)
						.Select(x => new RelativityObjectRef { ArtifactID = x })
						.ToList();

					if (!objectRefs.Any())
					{
						return;
					}

					var massDeleteByIds = new MassDeleteByObjectIdentifiersRequest
					{
						Objects = objectRefs,
					};
					await objectManager.DeleteAsync(workspace.ArtifactID, massDeleteByIds).ConfigureAwait(false);
				}
			}
		}
	}
}