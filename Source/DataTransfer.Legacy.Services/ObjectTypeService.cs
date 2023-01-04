using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	public class ObjectTypeService : BaseService, IObjectTypeService
	{
		private readonly ObjectTypeManager _objectTypeManager;
		private readonly ITraceGenerator _traceGenerator;

		public ObjectTypeService(IServiceContextFactory serviceContextFactory, ITraceGenerator traceGenerator) 
			: base(serviceContextFactory)
		{
			_objectTypeManager = new ObjectTypeManager();

			this._traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public Task<DataSetWrapper> RetrieveAllUploadableAsync(int workspaceID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.ObjectType.RetrieveAllUploadable", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _objectTypeManager.Query.RetrieveAllDynamicArtifactTypesWithSecurity(GetBaseServiceContext(workspaceID));
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<DataSetWrapper> RetrieveParentArtifactTypeIDAsync(int workspaceID, int artifactTypeID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.ObjectType.RetrieveParentArtifactTypeID", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _objectTypeManager.Query.RetrieveParentArtifactTypeID(GetBaseServiceContext(workspaceID), artifactTypeID);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}
	}
}