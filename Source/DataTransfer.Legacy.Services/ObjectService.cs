using System.Threading.Tasks;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	public class ObjectService : BaseService, IObjectService
	{
		private readonly ObjectManager _objectManager;

		public ObjectService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) : base(methodRunner, serviceContextFactory)
		{
			_objectManager = new ObjectManager();
		}

		public Task<DataSetWrapper> RetrieveArtifactIdOfMappedObjectAsync(int workspaceID, string textIdentifier, int artifactTypeID, string correlationID)
		{
			return ExecuteAsync(
				() => _objectManager.Query.RetrieveArtifactIdOfMappedObject(GetBaseServiceContext(workspaceID), textIdentifier, artifactTypeID),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveTextIdentifierOfMappedObjectAsync(int workspaceID, int artifactID, int artifactTypeID, string correlationID)
		{
			return ExecuteAsync(
				() => _objectManager.Query.RetrieveTextIdentifierOfMappedObject(GetBaseServiceContext(workspaceID), artifactID, artifactTypeID),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveArtifactIdOfMappedParentObjectAsync(int workspaceID, string textIdentifier, int artifactTypeID, string correlationID)
		{
			return ExecuteAsync(
				() => _objectManager.Query.RetrieveArtifactIdOfMappedParentObject(GetBaseServiceContext(workspaceID), textIdentifier, artifactTypeID),
				workspaceID, correlationID);
		}
	}
}