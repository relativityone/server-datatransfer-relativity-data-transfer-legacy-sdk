using System.Threading.Tasks;
using kCura.Data;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	public class DocumentService : BaseService, IDocumentService
	{
		public DocumentService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) : base(methodRunner, serviceContextFactory)
		{
		}

		public async Task<int[]> RetrieveAllUnsupportedOiFileIdsAsync(string correlationID)
		{
			return await ExecuteAsync(() =>
			{
				OIUnsupportedQuery unsupportedQuery = new OIUnsupportedQuery();
				var dataViewBase = unsupportedQuery.RetrieveAll(GetBaseServiceContext(AdminWorkspace));
				return DataViewBaseHelper.DataViewBaseToInt32Array(dataViewBase, "FileID");
			}, null, correlationID).ConfigureAwait(false);
		}
	}
}