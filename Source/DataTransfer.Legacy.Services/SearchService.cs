using System.Threading.Tasks;
using Relativity.Core.Service;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services
{
	public class SearchService : BaseService, ISearchService
	{
		private readonly SearchManager _searchManager;
		private readonly ViewManager _viewManager;

		public SearchService(IMethodRunner methodRunner, IServiceContextFactory serviceContextFactory) : base(methodRunner, serviceContextFactory)
		{
			_searchManager = new SearchManager();
			_viewManager = new ViewManager();
		}

		public async Task<bool[]> IsAssociatedSearchProviderAccessibleAsync(int workspaceID, int searchArtifactID, string correlationID)
		{
			return await ExecuteAsync(
				() => _searchManager.IsAssociatedSearchProviderAccessible(GetBaseServiceContext(workspaceID), searchArtifactID),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveNativesForSearchAsync(int workspaceID, string documentArtifactIDs, string correlationID)
		{
			return await ExecuteAsync(
				() => FileQuery.RetrieveNativesForDocuments(GetBaseServiceContext(workspaceID), documentArtifactIDs),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrievePdfForSearchAsync(int workspaceID, string documentArtifactIDs, string correlationID)
		{
			return await ExecuteAsync(
				() => FileQuery.RetrievePdfForDocuments(GetBaseServiceContext(workspaceID), documentArtifactIDs),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveFilesForDynamicObjectsAsync(int workspaceID, int fileFieldArtifactID, int[] objectIDs, string correlationID)
		{
			return await ExecuteAsync(
				() => FileQuery.RetrieveFilesForDynamicObjects(GetBaseServiceContext(workspaceID), fileFieldArtifactID, objectIDs),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveNativesForProductionAsync(int workspaceID, int productionArtifactID, string documentArtifactIDs, string correlationID)
		{
			return await ExecuteAsync(
				() => FileQuery.RetrieveNativesForProductionDocuments(GetBaseServiceContext(workspaceID), productionArtifactID, documentArtifactIDs),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveImagesForSearchAsync(int workspaceID, int[] documentArtifactIDs, string correlationID)
		{
			return await ExecuteAsync(
				() => FileQuery.RetrieveAllImagesForDocuments(GetBaseServiceContext(workspaceID), documentArtifactIDs),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveProducedImagesForDocumentAsync(int workspaceID, int documentArtifactID, string correlationID)
		{
			return await ExecuteAsync(
				() => FileQuery.RetrieveAllByDocumentArtifactIdAndType(GetBaseServiceContext(workspaceID), documentArtifactID, (int) FileType.StampedTif),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSetAsync(int workspaceID, int productionArtifactID, int[] documentArtifactIDs, string correlationID)
		{
			return await ExecuteAsync(
				() => FileQuery.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSet(GetBaseServiceContext(workspaceID), productionArtifactID, documentArtifactIDs),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveImagesByProductionIDsAndDocumentIDsForExportAsync(int workspaceID, int[] productionArtifactIDs, int[] documentArtifactIDs, string correlationID)
		{
			return await ExecuteAsync(
				() => FileQuery.RetrieveByProductionIDsAndDocumentIDsForExport(GetBaseServiceContext(workspaceID), productionArtifactIDs, documentArtifactIDs),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveViewsByContextArtifactIDAsync(int workspaceID, int artifactTypeID, bool isSearch, string correlationID)
		{
			return await ExecuteAsync(
				() => _viewManager.ExternalRetrieveViews(GetBaseServiceContext(workspaceID), artifactTypeID, isSearch),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveDefaultViewFieldsForIdListAsync(int workspaceID, int artifactTypeID, int[] artifactIdList, bool isProductionList, string correlationID)
		{
			return await ExecuteAsync(
				() => _searchManager.Query.RetrieveOrderedAvfLookupByArtifactIdList(GetBaseServiceContext(workspaceID), artifactTypeID, artifactIdList, isProductionList),
				workspaceID, correlationID).ConfigureAwait(false);
		}

		public async Task<DataSetWrapper> RetrieveAllExportableViewFieldsAsync(int workspaceID, int artifactTypeID, string correlationID)
		{
			return await ExecuteAsync(
				() => _searchManager.Query.RetrieveAllExportableViewFields(GetBaseServiceContext(workspaceID), artifactTypeID),
				workspaceID, correlationID).ConfigureAwait(false);
		}
	}
}