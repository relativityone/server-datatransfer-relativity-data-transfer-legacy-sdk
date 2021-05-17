using System.Linq;
using System.Threading.Tasks;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	public class ProductionService : BaseService, IProductionService
	{
		private readonly ProductionManager _productionManager;

		public ProductionService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) : base(methodRunner, serviceContextFactory)
		{
			_productionManager = new ProductionManager();
		}

		public async Task<object[][]> RetrieveBatesByProductionAndDocumentAsync(int workspaceID, int[] productionIDs, int[] documentIDs, string correlationID)
		{
			return await ExecuteAsync(() =>
			{
				var result = ProductionQuery.RetrieveBatesByProductionAndDocument(GetBaseServiceContext(workspaceID), GetUserAclMatrix(workspaceID), productionIDs, documentIDs);
				return result.Table.Select().Select(dr => dr.ItemArray).ToArray();
			}, workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveProducedByContextArtifactIDAsync(int workspaceID, string correlationID)
		{
			return await ExecuteAsync(
				() => _productionManager.ExternalRetrieveProduced(GetBaseServiceContext(workspaceID), GetUserAclMatrix(workspaceID)),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveImportEligibleByContextArtifactIDAsync(int workspaceID, string correlationID)
		{
			return await ExecuteAsync(
				() => _productionManager.ExternalRetrieveImportEligible(GetBaseServiceContext(workspaceID)),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task DoPostImportProcessingAsync(int workspaceID, int productionArtifactID, string correlationID)
		{
			await ExecuteAsync(
				() => _productionManager.ExternalDoPostImportProcessing(GetBaseServiceContext(workspaceID), productionArtifactID),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task DoPreImportProcessingAsync(int workspaceID, int productionArtifactID, string correlationID)
		{
			await ExecuteAsync(
				() => _productionManager.ExternalDoPreImportProcessing(GetBaseServiceContext(workspaceID), productionArtifactID),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<ProductionInfo> ReadAsync(int workspaceID, int productionArtifactID, string correlationID)
		{
			return await ExecuteAsync(
				() => _productionManager.ReadInfo(GetBaseServiceContext(workspaceID), productionArtifactID).Map<ProductionInfo>(),
				workspaceID, correlationID).ConfigureAwait(false);
		}
	}
}