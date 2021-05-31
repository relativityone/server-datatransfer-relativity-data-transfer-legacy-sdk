using System.Threading.Tasks;
using Castle.Core;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	[Interceptor(typeof(LogInterceptor))]
	public class SearchService : BaseService, ISearchService
	{
		private readonly SearchManager _searchManager;
		private readonly ViewManager _viewManager;

		public SearchService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) 
			: base(methodRunner, serviceContextFactory)
		{
			_searchManager = new SearchManager();
			_viewManager = new ViewManager();
		}

		public Task<bool[]> IsAssociatedSearchProviderAccessibleAsync(int workspaceID, int searchArtifactID, string correlationID)
		{
			return ExecuteAsync(
				() => _searchManager.IsAssociatedSearchProviderAccessible(GetBaseServiceContext(workspaceID), searchArtifactID),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveNativesForSearchAsync(int workspaceID, string documentArtifactIDs, string correlationID)
		{
			return ExecuteAsync(
				() => FileQuery.RetrieveNativesForDocuments(GetBaseServiceContext(workspaceID), documentArtifactIDs),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrievePdfForSearchAsync(int workspaceID, string documentArtifactIDs, string correlationID)
		{
			return ExecuteAsync(
				() => FileQuery.RetrievePdfForDocuments(GetBaseServiceContext(workspaceID), documentArtifactIDs),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveFilesForDynamicObjectsAsync(int workspaceID, int fileFieldArtifactID, int[] objectIDs, string correlationID)
		{
			return ExecuteAsync(
				() => FileQuery.RetrieveFilesForDynamicObjects(GetBaseServiceContext(workspaceID), fileFieldArtifactID, objectIDs),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveNativesForProductionAsync(int workspaceID, int productionArtifactID, string documentArtifactIDs, string correlationID)
		{
			return ExecuteAsync(
				() => FileQuery.RetrieveNativesForProductionDocuments(GetBaseServiceContext(workspaceID), productionArtifactID, documentArtifactIDs),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveImagesForSearchAsync(int workspaceID, int[] documentArtifactIDs, string correlationID)
		{
			return ExecuteAsync(
				() => FileQuery.RetrieveAllImagesForDocuments(GetBaseServiceContext(workspaceID), documentArtifactIDs),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveProducedImagesForDocumentAsync(int workspaceID, int documentArtifactID, string correlationID)
		{
			return ExecuteAsync(
				() => FileQuery.RetrieveAllByDocumentArtifactIdAndType(GetBaseServiceContext(workspaceID), documentArtifactID, (int) FileType.StampedTif),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSetAsync(int workspaceID, int productionArtifactID, int[] documentArtifactIDs, string correlationID)
		{
			return ExecuteAsync(
				() => FileQuery.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSet(GetBaseServiceContext(workspaceID), productionArtifactID, documentArtifactIDs),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveImagesByProductionIDsAndDocumentIDsForExportAsync(int workspaceID, int[] productionArtifactIDs, int[] documentArtifactIDs, string correlationID)
		{
			return ExecuteAsync(
				() => FileQuery.RetrieveByProductionIDsAndDocumentIDsForExport(GetBaseServiceContext(workspaceID), productionArtifactIDs, documentArtifactIDs),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveViewsByContextArtifactIDAsync(int workspaceID, int artifactTypeID, bool isSearch, string correlationID)
		{
			return ExecuteAsync(
				() => _viewManager.ExternalRetrieveViews(GetBaseServiceContext(workspaceID), artifactTypeID, isSearch),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveDefaultViewFieldsForIdListAsync(int workspaceID, int artifactTypeID, int[] artifactIdList, bool isProductionList, string correlationID)
		{
			return ExecuteAsync(
				() => _searchManager.Query.RetrieveOrderedAvfLookupByArtifactIdList(GetBaseServiceContext(workspaceID), artifactTypeID, artifactIdList, isProductionList),
				workspaceID, correlationID);
		}

		public Task<DataSetWrapper> RetrieveAllExportableViewFieldsAsync(int workspaceID, int artifactTypeID, string correlationID)
		{
			return ExecuteAsync(
				() => _searchManager.Query.RetrieveAllExportableViewFields(GetBaseServiceContext(workspaceID), artifactTypeID),
				workspaceID, correlationID);
		}
	}
}