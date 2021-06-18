using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(ToggleCheckInterceptor))]
	[Interceptor(typeof(PermissionCheckInterceptor))]
	[Interceptor(typeof(LogInterceptor))]
	[Interceptor(typeof(MetricsInterceptor))]
	[Interceptor(typeof(UnhandledExceptionInterceptor))]
	public class SearchService : BaseService, ISearchService
	{
		private readonly SearchManager _searchManager;
		private readonly ViewManager _viewManager;

		public SearchService(IServiceContextFactory serviceContextFactory) 
			: base(serviceContextFactory)
		{
			_searchManager = new SearchManager();
			_viewManager = new ViewManager();
		}

		public Task<bool[]> IsAssociatedSearchProviderAccessibleAsync(int workspaceID, int searchArtifactID, string correlationID)
		{
			var result = _searchManager.IsAssociatedSearchProviderAccessible(GetBaseServiceContext(workspaceID), searchArtifactID);
			return Task.FromResult(result);
		}

		public Task<DataSetWrapper> RetrieveNativesForSearchAsync(int workspaceID, string documentArtifactIDs, string correlationID)
		{
			var result = FileQuery.RetrieveNativesForDocuments(GetBaseServiceContext(workspaceID), documentArtifactIDs);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}

		public Task<DataSetWrapper> RetrievePdfForSearchAsync(int workspaceID, string documentArtifactIDs, string correlationID)
		{
			var result = FileQuery.RetrievePdfForDocuments(GetBaseServiceContext(workspaceID), documentArtifactIDs);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}

		public Task<DataSetWrapper> RetrieveFilesForDynamicObjectsAsync(int workspaceID, int fileFieldArtifactID, int[] objectIDs, string correlationID)
		{
			var result = FileQuery.RetrieveFilesForDynamicObjects(GetBaseServiceContext(workspaceID), fileFieldArtifactID, objectIDs);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}

		public Task<DataSetWrapper> RetrieveNativesForProductionAsync(int workspaceID, int productionArtifactID, string documentArtifactIDs, string correlationID)
		{
			var result = FileQuery.RetrieveNativesForProductionDocuments(GetBaseServiceContext(workspaceID), productionArtifactID, documentArtifactIDs);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}

		public Task<DataSetWrapper> RetrieveImagesForSearchAsync(int workspaceID, int[] documentArtifactIDs, string correlationID)
		{
			var result = FileQuery.RetrieveAllImagesForDocuments(GetBaseServiceContext(workspaceID), documentArtifactIDs);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}

		public Task<DataSetWrapper> RetrieveProducedImagesForDocumentAsync(int workspaceID, int documentArtifactID, string correlationID)
		{
			var result = FileQuery.RetrieveAllByDocumentArtifactIdAndType(GetBaseServiceContext(workspaceID), documentArtifactID, (int) FileType.StampedTif);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}

		public Task<DataSetWrapper> RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSetAsync(int workspaceID, int productionArtifactID, int[] documentArtifactIDs, string correlationID)
		{
			var result = FileQuery.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSet(GetBaseServiceContext(workspaceID), productionArtifactID, documentArtifactIDs);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}

		public Task<DataSetWrapper> RetrieveImagesByProductionIDsAndDocumentIDsForExportAsync(int workspaceID, int[] productionArtifactIDs, int[] documentArtifactIDs, string correlationID)
		{
			var result = FileQuery.RetrieveByProductionIDsAndDocumentIDsForExport(GetBaseServiceContext(workspaceID), productionArtifactIDs, documentArtifactIDs);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}

		public Task<DataSetWrapper> RetrieveViewsByContextArtifactIDAsync(int workspaceID, int artifactTypeID, bool isSearch, string correlationID)
		{
			var result = _viewManager.ExternalRetrieveViews(GetBaseServiceContext(workspaceID), artifactTypeID, isSearch);
			return Task.FromResult(new DataSetWrapper(result));
		}

		public Task<DataSetWrapper> RetrieveDefaultViewFieldsForIdListAsync(int workspaceID, int artifactTypeID, int[] artifactIdList, bool isProductionList, string correlationID)
		{
			var result = _searchManager.Query.RetrieveOrderedAvfLookupByArtifactIdList(GetBaseServiceContext(workspaceID), artifactTypeID, artifactIdList, isProductionList);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));

		}

		public Task<DataSetWrapper> RetrieveAllExportableViewFieldsAsync(int workspaceID, int artifactTypeID, string correlationID)
		{
			var result = _searchManager.Query.RetrieveAllExportableViewFields(GetBaseServiceContext(workspaceID), artifactTypeID);
			return Task.FromResult(new DataSetWrapper(result.ToDataSet()));
		}
	}
}