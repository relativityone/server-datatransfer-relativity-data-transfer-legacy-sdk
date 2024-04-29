using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Interfaces.Shared;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;

namespace MassImport.NUnit.Integration.Helpers
{
	internal static class RdoHelper
	{
		public const int WorkspaceArtifactTypeId = 8;
		public static readonly string ArtifactId = "ArtifactID";
		public static readonly string ParentArtifactId = "ParentArtifactID";

		/// <summary>
		/// Deletes all RDOs of a given type from a test workspace.
		/// </summary>
		/// <param name="parameters">Test context parameters.</param>
		/// <param name="artifactTypeID">Type of objects to delete.</param>
		/// <returns><see cref="Task"/> which completes when all RDOs are deleted.</returns>
		public static async Task DeleteAllObjectsByTypeAsync(IntegrationTestParameters parameters, TestWorkspace workspace, int artifactTypeID)
		{
			const int DeleteBatchSize = 250;

			// Deleting objects in a small batches is more stable than deleting all objects of a given type at one go.
			// Please see https://jira.kcura.com/browse/REL-496822 and https://jira.kcura.com/browse/REL-478746 for details.
			using (var objectManager = ServiceHelper.GetServiceProxy<IObjectManager>(parameters))
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
						.QuerySlimAsync(workspace.WorkspaceId, queryAllObjectsRequest, start: 0, length: DeleteBatchSize)
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
					await objectManager.DeleteAsync(workspace.WorkspaceId, massDeleteByIds).ConfigureAwait(false);
				}
			}
		}

		public static async Task<IList<RelativityObject>> QueryDocuments(
			IntegrationTestParameters parameters,
			TestWorkspace workspace,
			string[] fieldNames)
		{
			var query = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = WellKnownFields.DocumentArtifactTypeId },
				Fields = fieldNames.Select(x => new FieldRef { Name = x }).ToArray()
			};

			const int CountLimit = 10_000;

			QueryResult result;
			using (var objectManager = ServiceHelper.GetServiceProxy<IObjectManager>(parameters))
			{
				result = await objectManager.QueryAsync(workspace.WorkspaceId, query, 0, CountLimit).ConfigureAwait(false);
			}

			return result.Objects;

		}

		public static async Task<Dictionary<string, Dictionary<string, object>>> ReadObjects(
			IntegrationTestParameters parameters,
			TestWorkspace workspace,
			int objectTypeId,
			string identifierFieldName,
			string[] fieldNames)
		{
			const int CountLimit = 10_000;

			var allFields = new[] { identifierFieldName }.Concat(fieldNames);
			var query = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = objectTypeId },
				Fields = allFields.Select(x => new FieldRef { Name = x }).ToArray()
			};

			QueryResult result;
			using (var objectManager = ServiceHelper.GetServiceProxy<IObjectManager>(parameters))
			{
				result = await objectManager.QueryAsync(workspace.WorkspaceId, query, 0, CountLimit).ConfigureAwait(false);
			}

			if (result.TotalCount > result.ResultCount)
			{
				throw new InvalidOperationException("Workspace contains more RDOs than supported limit.");
			}

			Dictionary<string, Dictionary<string, object>> returnValue = new Dictionary<string, Dictionary<string, object>>();
			foreach (var rdo in result.Objects)
			{
				string identifier = rdo[identifierFieldName].Value as string;

				if (!string.IsNullOrEmpty(identifier))
				{
					var fields = new Dictionary<string, object>();

					for (int i = 0; i < fieldNames.Length; i++)
					{
						var value = rdo[fieldNames[i]].Value;
						if (value is Choice valueAsChoice)
						{
							fields[fieldNames[i]] = valueAsChoice.ArtifactID;
						}
						else if (value is List<Choice> valueAsMultiChoice)
						{
							fields[fieldNames[i]] = valueAsMultiChoice.Select(x => x.ArtifactID).ToArray();
						}
						else if (value is RelativityObjectValue valueAsObject)
						{
							fields[fieldNames[i]] = valueAsObject.Name;
						}
						else if (value is List<RelativityObjectValue> valuesAsObject)
						{
							fields[fieldNames[i]] = valuesAsObject.Select(x => x.ArtifactID).ToArray();
						}
						else
						{
							fields[fieldNames[i]] = value;
						}
					}

					fields[ParentArtifactId] = rdo.ParentObject.ArtifactID;
					fields[ArtifactId] = rdo.ArtifactID;

					returnValue[identifier] = fields;
				}

			}

			return returnValue;
		}

		public static Task<int> CreateObjectTypeAsync(
			IntegrationTestParameters parameters,
			TestWorkspace testWorkspace,
			string objectTypeName) => CreateObjectTypeAsync(
			parameters,
			testWorkspace,
			objectTypeName,
			WorkspaceArtifactTypeId);

		public static async Task<int> CreateObjectTypeAsync(
			IntegrationTestParameters parameters,
			TestWorkspace testWorkspace,
			string objectTypeName,
			int parentArtifactId)
		{
			using (var objectManager = ServiceHelper.GetServiceProxy<IObjectTypeManager>(parameters))
			{
				var request = new ObjectTypeRequest
				{
					Name = objectTypeName,
					ParentObjectType = new Securable<ObjectTypeIdentifier>(new ObjectTypeIdentifier { ArtifactTypeID = parentArtifactId }),
				};

				int newObjectId = await objectManager.CreateAsync(testWorkspace.WorkspaceId, request).ConfigureAwait(false);

				ObjectTypeResponse objectTypeResponse = await objectManager.ReadAsync(testWorkspace.WorkspaceId, newObjectId).ConfigureAwait(false);
				return objectTypeResponse.ArtifactTypeID;
			}
		}

		
		public static async Task<Dictionary<string, int>> CreateObjectsAsync(
			IntegrationTestParameters parameters,
			TestWorkspace testWorkspace,
			int artifactTypeId,
			List<string> objectsNames)
		{
			var request = new MassCreateRequest
			{
				Fields = new List<FieldRef>
				{
					new FieldRef { Name = "Name" },
				},
				ObjectType = new ObjectTypeRef()
				{
					ArtifactTypeID = artifactTypeId,
				},
				ValueLists = objectsNames.Select(x => new List<object> { x }).ToList(),
			};

			using (var objectManager = ServiceHelper.GetServiceProxy<IObjectManager>(parameters))
			{
				var response = await objectManager.CreateAsync(testWorkspace.WorkspaceId, request).ConfigureAwait(false);
				response.Success.Should().BeTrue("because it should create RDOs");

				return response.Objects
					.Zip(objectsNames, (relativityObject, name) => (name, relativityObject.ArtifactID))
					.ToDictionary(x => x.name, x => x.ArtifactID);
			}
		}
	}
}
