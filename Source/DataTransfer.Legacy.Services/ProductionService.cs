using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.Export;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	public class ProductionService : BaseService, IProductionService
	{
		private readonly ProductionManager _productionManager;

		public ProductionService(IServiceContextFactory serviceContextFactory) 
			: base(serviceContextFactory)
		{
			_productionManager = new ProductionManager();
		}

		public Task<ExportDataWrapper> RetrieveBatesByProductionAndDocumentAsync(int workspaceID, int[] productionIDs,
			int[] documentIDs, string correlationID)
		{
			var resultAsDataView = ProductionQuery.RetrieveBatesByProductionAndDocument(
				GetBaseServiceContext(workspaceID), GetUserAclMatrix(workspaceID), productionIDs, documentIDs);
			var resultAsObjectArrays =
				ToObjectArrays(resultAsDataView, ProductionDocumentBatesHelper.ToSerializableObjectArray);
			var result = new ExportDataWrapper(resultAsObjectArrays);
			return Task.FromResult(result);
		}

		public Task<DataSetWrapper> RetrieveProducedByContextArtifactIDAsync(int workspaceID, string correlationID)
		{
			var result = _productionManager.ExternalRetrieveProduced(GetBaseServiceContext(workspaceID), GetUserAclMatrix(workspaceID));
			return Task.FromResult(result != null ? new DataSetWrapper(result) : null);
		}

		public Task<DataSetWrapper> RetrieveImportEligibleByContextArtifactIDAsync(int workspaceID, string correlationID)
		{
			var result = _productionManager.ExternalRetrieveImportEligible(GetBaseServiceContext(workspaceID));
			return Task.FromResult(result != null ? new DataSetWrapper(result) : null);
		}

		public Task DoPostImportProcessingAsync(int workspaceID, int productionArtifactID, string correlationID)
		{
			_productionManager.ExternalDoPostImportProcessing(GetBaseServiceContext(workspaceID), productionArtifactID);
			return Task.CompletedTask;
		}

		public Task DoPreImportProcessingAsync(int workspaceID, int productionArtifactID, string correlationID)
		{
			_productionManager.ExternalDoPreImportProcessing(GetBaseServiceContext(workspaceID), productionArtifactID);
			return Task.CompletedTask;
		}

		public Task<ProductionInfo> ReadAsync(int workspaceID, int productionArtifactID, string correlationID)
		{
			var result = _productionManager.ReadInfo(GetBaseServiceContext(workspaceID), productionArtifactID).Map<ProductionInfo>();
			return Task.FromResult(result);
		}

        private static object[][] ToObjectArrays(kCura.Data.DataView dataView, Func<DataRow, object[]> transformer)
        {
            if (transformer == null)
            {
                transformer = row => row.ItemArray;
            }

            return dataView.Table.Select().Select(transformer).ToArray();
        }
	}
}