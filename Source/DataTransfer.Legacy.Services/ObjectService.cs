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
	public class ObjectService : BaseService, IObjectService
	{
		private readonly ObjectManager _objectManager;
		private readonly ITraceGenerator _traceGenerator;

		public ObjectService(IServiceContextFactory serviceContextFactory, ITraceGenerator traceGenerator) 
			: base(serviceContextFactory)
		{
			_objectManager = new ObjectManager();

			this._traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public Task<DataSetWrapper> RetrieveArtifactIdOfMappedObjectAsync(int workspaceID, string textIdentifier, int artifactTypeID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Object.RetrieveArtifactIdOfMappedObject", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _objectManager.Query.RetrieveArtifactIdOfMappedObject(GetBaseServiceContext(workspaceID), textIdentifier, artifactTypeID);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<DataSetWrapper> RetrieveTextIdentifierOfMappedObjectAsync(int workspaceID, int artifactID, int artifactTypeID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Object.RetrieveTextIdentifierOfMappedObject", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _objectManager.Query.RetrieveTextIdentifierOfMappedObject(GetBaseServiceContext(workspaceID), artifactID, artifactTypeID);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<DataSetWrapper> RetrieveArtifactIdOfMappedParentObjectAsync(int workspaceID, string textIdentifier, int artifactTypeID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Object.RetrieveArtifactIdOfMappedParentObject", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _objectManager.Query.RetrieveArtifactIdOfMappedParentObject(GetBaseServiceContext(workspaceID), textIdentifier, artifactTypeID);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}
	}
}