using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("Search Manager replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("search")]
	public interface ISearchService : IDisposable
	{
		[HttpPost]
		[Route("IsAssociatedSearchProviderAccessibleAsync")]
		Task<bool[]> IsAssociatedSearchProviderAccessibleAsync(int workspaceID, int searchArtifactID, string correlationID);

		[HttpPost]
		[Route("RetrieveNativesForSearchAsync")]
		Task<DataSetWrapper> RetrieveNativesForSearchAsync(int workspaceID, string documentArtifactIDs, string correlationID);

		[HttpPost]
		[Route("RetrievePdfForSearchAsync")]
		Task<DataSetWrapper> RetrievePdfForSearchAsync(int workspaceID, string documentArtifactIDs, string correlationID);

		[HttpPost]
		[Route("RetrieveNativesForProductionAsync")]
		Task<DataSetWrapper> RetrieveFilesForDynamicObjectsAsync(int workspaceID, int fileFieldArtifactID, int[] objectIDs, string correlationID);

		[HttpPost]
		[Route("")]
		Task<DataSetWrapper> RetrieveNativesForProductionAsync(int workspaceID, int productionArtifactID, string documentArtifactIDs, string correlationID);

		[HttpPost]
		[Route("RetrieveImagesForSearchAsync")]
		Task<DataSetWrapper> RetrieveImagesForSearchAsync(int workspaceID, int[] documentArtifactIDs, string correlationID);

		[HttpPost]
		[Route("RetrieveProducedImagesForDocumentAsync")]
		Task<DataSetWrapper> RetrieveProducedImagesForDocumentAsync(int workspaceID, int documentArtifactID, string correlationID);

		[HttpPost]
		[Route("RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSetAsync")]
		Task<DataSetWrapper> RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSetAsync(int workspaceID, int productionArtifactID, int[] documentArtifactIDs, string correlationID);

		[HttpPost]
		[Route("RetrieveImagesByProductionIDsAndDocumentIDsForExportAsync")]
		Task<DataSetWrapper> RetrieveImagesByProductionIDsAndDocumentIDsForExportAsync(int workspaceID, int[] productionArtifactIDs, int[] documentArtifactIDs, string correlationID);

		[HttpPost]
		[Route("RetrieveViewsByContextArtifactIDAsync")]
		Task<DataSetWrapper> RetrieveViewsByContextArtifactIDAsync(int workspaceID, int artifactTypeID, bool isSearch, string correlationID);

		[HttpPost]
		[Route("RetrieveDefaultViewFieldsForIdListAsync")]
		Task<DataSetWrapper> RetrieveDefaultViewFieldsForIdListAsync(int workspaceID, int artifactTypeID, int[] artifactIdList, bool isProductionList, string correlationID);

		[HttpPost]
		[Route("RetrieveAllExportableViewFieldsAsync")]
		Task<DataSetWrapper> RetrieveAllExportableViewFieldsAsync(int workspaceID, int artifactTypeID, string correlationID);
	}
}