using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Runners;
using Relativity.Export;

namespace Relativity.DataTransfer.Legacy.Services
{
	public class ProductionService : BaseService, IProductionService
	{
		private readonly ProductionManager _productionManager;

		public ProductionService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) : base(methodRunner, serviceContextFactory)
		{
			_productionManager = new ProductionManager();
		}

		public Task<object[][]> RetrieveBatesByProductionAndDocumentAsync(int workspaceID, int[] productionIDs, int[] documentIDs, string correlationID)
		{
			return ExecuteAsync(() =>
			{
				var result = ProductionQuery.RetrieveBatesByProductionAndDocument(GetBaseServiceContext(workspaceID), GetUserAclMatrix(workspaceID), productionIDs, documentIDs);
				return ToObjectArrays(result, ProductionDocumentBatesHelper.ToSerializableObjectArray);
                
			}, workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveProducedByContextArtifactIDAsync(int workspaceID, string correlationID)
		{
			return ExecuteAsync(
				() => _productionManager.ExternalRetrieveProduced(GetBaseServiceContext(workspaceID), GetUserAclMatrix(workspaceID)),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveImportEligibleByContextArtifactIDAsync(int workspaceID, string correlationID)
		{
			return ExecuteAsync(
				() => _productionManager.ExternalRetrieveImportEligible(GetBaseServiceContext(workspaceID)),
				workspaceID, correlationID);
		}

		public Task DoPostImportProcessingAsync(int workspaceID, int productionArtifactID, string correlationID)
		{
			return ExecuteAsync(
				() => _productionManager.ExternalDoPostImportProcessing(GetBaseServiceContext(workspaceID), productionArtifactID),
				workspaceID, correlationID);
		}

		public Task DoPreImportProcessingAsync(int workspaceID, int productionArtifactID, string correlationID)
		{
			return ExecuteAsync(
				() => _productionManager.ExternalDoPreImportProcessing(GetBaseServiceContext(workspaceID), productionArtifactID),
				workspaceID, correlationID);
		}

		public Task<ProductionInfo> ReadAsync(int workspaceID, int productionArtifactID, string correlationID)
		{
			return ExecuteAsync(
				() => _productionManager.ReadInfo(GetBaseServiceContext(workspaceID), productionArtifactID).Map<ProductionInfo>(),
				workspaceID, correlationID);
		}

        private static object[][] ToObjectArrays(kCura.Data.DataView dv, Func<DataRow, object[]> transformer)
        {
            if (transformer == null)
            {
                transformer = row => row.ItemArray;
            }

            return dv.Table.Select().Select(transformer).ToArray();
        }
	}
}