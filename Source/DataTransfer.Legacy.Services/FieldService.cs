using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Observability;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(DistributedTracingInterceptor))]
	public class FieldService : BaseService, IFieldService
	{
		private readonly FieldManager _fieldManager;

		public FieldService(IServiceContextFactory serviceContextFactory)
			: base(serviceContextFactory)
		{
			_fieldManager = new FieldManager();
		}

		public Task<Field> ReadAsync(int workspaceID, int fieldArtifactID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag("r1.workspace.id", workspaceID);

			var result = _fieldManager.Read(GetBaseServiceContext(workspaceID), fieldArtifactID).Map<Field>();
			return Task.FromResult(result);
		}

		public Task<DataSetWrapper> RetrieveAllMappableAsync(int workspaceID, int artifactTypeID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag("r1.workspace.id", workspaceID);

			var result = FieldManagerFoundation.Query.RetrieveAllMappable(GetBaseServiceContext(workspaceID), artifactTypeID);
			return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
		}

		public Task<DataSetWrapper> RetrievePotentialBeginBatesFieldsAsync(int workspaceID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag("r1.workspace.id", workspaceID);

			var result = FieldQuery.RetrievePotentialBeginBatesFields(GetBaseServiceContext(workspaceID));
			return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
		}

		public Task<bool> IsFieldIndexedAsync(int workspaceID, int fieldArtifactID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag("r1.workspace.id", workspaceID);

			var result = _fieldManager.IsFieldIndexed(GetBaseServiceContext(workspaceID), fieldArtifactID);
			return Task.FromResult(result);
		}
	}
}