using System.Threading.Tasks;
using Castle.Core;
using kCura.Data;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(DistributedTracingInterceptor))]
	public class DocumentService : BaseService, IDocumentService
	{
		public DocumentService(IServiceContextFactory serviceContextFactory) : base(serviceContextFactory) { }

		public Task<int[]> RetrieveAllUnsupportedOiFileIdsAsync(string correlationID)
		{
			var unsupportedQuery = new OIUnsupportedQuery();
			var dataViewBase = unsupportedQuery.RetrieveAll(GetBaseServiceContext(AdminWorkspace));
			var result = DataViewBaseHelper.DataViewBaseToInt32Array(dataViewBase, "FileID");
			return Task.FromResult(result);
		}
	}
}