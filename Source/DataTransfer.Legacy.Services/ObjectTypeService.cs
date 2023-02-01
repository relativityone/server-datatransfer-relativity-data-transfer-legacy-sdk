using System.Diagnostics;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using TelemetryConstants = DataTransfer.Legacy.MassImport.RelEyeTelemetry.TelemetryConstants;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(DistributedTracingInterceptor))]
	public class ObjectTypeService : BaseService, IObjectTypeService
	{
		private readonly ObjectTypeManager _objectTypeManager;

		public ObjectTypeService(IServiceContextFactory serviceContextFactory)
			: base(serviceContextFactory)
		{
			_objectTypeManager = new ObjectTypeManager();
		}

		public Task<DataSetWrapper> RetrieveAllUploadableAsync(int workspaceID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, workspaceID);

			var result = _objectTypeManager.Query.RetrieveAllDynamicArtifactTypesWithSecurity(GetBaseServiceContext(workspaceID));
			return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
		}

		public Task<DataSetWrapper> RetrieveParentArtifactTypeIDAsync(int workspaceID, int artifactTypeID, string correlationID)
		{
			var activity = Activity.Current;
			activity?.SetTag(TelemetryConstants.AttributeNames.R1WorkspaceID, workspaceID);

			var result = _objectTypeManager.Query.RetrieveParentArtifactTypeID(GetBaseServiceContext(workspaceID), artifactTypeID);
			return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
		}
	}
}