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
	public class FieldService : BaseService, IFieldService
	{
		private readonly FieldManager _fieldManager;
		private readonly ITraceGenerator _traceGenerator;

		public FieldService(IServiceContextFactory serviceContextFactory, ITraceGenerator traceGenerator) 
			: base(serviceContextFactory)
		{
			_fieldManager = new FieldManager();
			this._traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public Task<Field> ReadAsync(int workspaceID, int fieldArtifactID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Field.Read", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _fieldManager.Read(GetBaseServiceContext(workspaceID), fieldArtifactID).Map<Field>();
				return Task.FromResult(result);
			}
		}

		public Task<DataSetWrapper> RetrieveAllMappableAsync(int workspaceID, int artifactTypeID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Field.RetrieveAllMappable", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = FieldManagerFoundation.Query.RetrieveAllMappable(GetBaseServiceContext(workspaceID), artifactTypeID);
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<DataSetWrapper> RetrievePotentialBeginBatesFieldsAsync(int workspaceID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Field.RetrievePotentialBeginBatesFields", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = FieldQuery.RetrievePotentialBeginBatesFields(GetBaseServiceContext(workspaceID));
				return Task.FromResult(result != null ? new DataSetWrapper(result.ToDataSet()) : null);
			}
		}

		public Task<bool> IsFieldIndexedAsync(int workspaceID, int fieldArtifactID, string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Field.IsFieldIndexed", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);
				activity?.SetTag("r1.workspace.id", workspaceID);

				var result = _fieldManager.IsFieldIndexed(GetBaseServiceContext(workspaceID), fieldArtifactID);
				return Task.FromResult(result);
			}
		}
	}
}