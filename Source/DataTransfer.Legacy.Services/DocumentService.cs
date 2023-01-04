using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Castle.Core;
using kCura.Data;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using System.ServiceModel.Syndication;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	public class DocumentService : BaseService, IDocumentService
	{
		private readonly ITraceGenerator _traceGenerator;

		public DocumentService(IServiceContextFactory serviceContextFactory, ITraceGenerator traceGenerator) 
			: base(serviceContextFactory)
		{
			this._traceGenerator = traceGenerator ?? throw new ArgumentNullException(nameof(traceGenerator));

			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public Task<int[]> RetrieveAllUnsupportedOiFileIdsAsync(string correlationID)
		{
			using (var activity = _traceGenerator.GetActivitySurce()?.StartActivity("DataTransfer.Legacy.Kepler.Api.Document.RetrieveAllUnsupportedOiFileIds", ActivityKind.Server))
			{
				_traceGenerator.SetSystemTags(activity);

				activity?.SetTag("job.id", correlationID);

				var unsupportedQuery = new OIUnsupportedQuery();
				var dataViewBase = unsupportedQuery.RetrieveAll(GetBaseServiceContext(AdminWorkspace));
				var result = DataViewBaseHelper.DataViewBaseToInt32Array(dataViewBase, "FileID");
				return Task.FromResult(result);
			}
		}
	}
}