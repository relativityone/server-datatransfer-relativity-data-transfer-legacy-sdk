using System.Threading.Tasks;
using Castle.Core;
using kCura.Data;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	public class DocumentService : BaseService, IDocumentService
	{
		public DocumentService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) 
			: base(methodRunner, serviceContextFactory)
		{
		}

		public Task<int[]> RetrieveAllUnsupportedOiFileIdsAsync(string correlationID)
		{
			return ExecuteAsync(() =>
			{
				OIUnsupportedQuery unsupportedQuery = new OIUnsupportedQuery();
				var dataViewBase = unsupportedQuery.RetrieveAll(GetBaseServiceContext(AdminWorkspace));
				return DataViewBaseHelper.DataViewBaseToInt32Array(dataViewBase, "FileID");
			}, null, correlationID);
		}
	}
}