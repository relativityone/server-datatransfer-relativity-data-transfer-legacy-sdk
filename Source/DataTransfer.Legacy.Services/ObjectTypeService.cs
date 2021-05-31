using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(LogInterceptor))]
	public class ObjectTypeService : BaseService, IObjectTypeService
	{
		private readonly ObjectTypeManager _objectTypeManager;

		public ObjectTypeService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) 
			: base(methodRunner, serviceContextFactory)
		{
			_objectTypeManager = new ObjectTypeManager();
		}

		public Task<DataSetWrapper> RetrieveAllUploadableAsync(int workspaceID, string correlationID)
		{
			return ExecuteAsync(
				() => _objectTypeManager.Query.RetrieveAllDynamicArtifactTypesWithSecurity(GetBaseServiceContext(workspaceID)),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveParentArtifactTypeIDAsync(int workspaceID, int artifactTypeID, string correlationID)
		{
			return ExecuteAsync(
				() => _objectTypeManager.Query.RetrieveParentArtifactTypeID(GetBaseServiceContext(workspaceID), artifactTypeID),
				workspaceID, correlationID);
		}
	}
}