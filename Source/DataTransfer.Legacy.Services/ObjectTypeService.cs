using System.Threading.Tasks;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	public class ObjectTypeService : BaseService, IObjectTypeService
	{
		private readonly ObjectTypeManager _objectTypeManager;

		public ObjectTypeService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) : base(methodRunner, serviceContextFactory)
		{
			_objectTypeManager = new ObjectTypeManager();
		}

		public async Task<DataSetWrapper> RetrieveAllUploadableAsync(int workspaceID, string correlationID)
		{
			return await ExecuteAsync(
				() => _objectTypeManager.Query.RetrieveAllDynamicArtifactTypesWithSecurity(GetBaseServiceContext(workspaceID)),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveParentArtifactTypeIDAsync(int workspaceID, int artifactTypeID, string correlationID)
		{
			return await ExecuteAsync(
				() => _objectTypeManager.Query.RetrieveParentArtifactTypeID(GetBaseServiceContext(workspaceID), artifactTypeID),
				workspaceID, correlationID).ConfigureAwait(false);
		}
	}
}