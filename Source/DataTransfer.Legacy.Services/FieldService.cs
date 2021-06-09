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
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	public class FieldService : BaseService, IFieldService
	{
		private readonly FieldManager _fieldManager;

		public FieldService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) 
			: base(methodRunner, serviceContextFactory)
		{
			_fieldManager = new FieldManager();
		}

		public Task<Field> ReadAsync(int workspaceID, int fieldArtifactID, string correlationID)
		{
			return ExecuteAsync(
				() => _fieldManager.Read(GetBaseServiceContext(workspaceID), fieldArtifactID).Map<Field>(),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveAllMappableAsync(int workspaceID, int artifactTypeID, string correlationID)
		{
			return ExecuteAsync(
				() => FieldManagerFoundation.Query.RetrieveAllMappable(GetBaseServiceContext(workspaceID), artifactTypeID),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrievePotentialBeginBatesFieldsAsync(int workspaceID, string correlationID)
		{
			return ExecuteAsync(
				() => FieldQuery.RetrievePotentialBeginBatesFields(GetBaseServiceContext(workspaceID)),
				workspaceID, correlationID);
		}

		public Task<bool> IsFieldIndexedAsync(int workspaceID, int fieldArtifactID, string correlationID)
		{
			return ExecuteAsync(
				() => _fieldManager.IsFieldIndexed(GetBaseServiceContext(workspaceID), fieldArtifactID),
				workspaceID, correlationID);
		}
	}
}