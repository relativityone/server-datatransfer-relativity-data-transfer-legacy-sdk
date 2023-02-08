using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("Production Manager replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("production")]
	public interface IProductionService : IDisposable
	{
		[HttpPost]
		[Route("RetrieveBatesByProductionAndDocumentAsync")]
		Task<ExportDataWrapper> RetrieveBatesByProductionAndDocumentAsync(int workspaceID, int[] productionIDs, int[] documentIDs, string correlationID);

		[HttpPost]
		[Route("RetrieveProducedByContextArtifactIDAsync")]
		Task<DataSetWrapper> RetrieveProducedByContextArtifactIDAsync(int workspaceID, string correlationID);

		[HttpPost]
		[Route("RetrieveImportEligibleByContextArtifactIDAsync")]
		Task<DataSetWrapper> RetrieveImportEligibleByContextArtifactIDAsync(int workspaceID, string correlationID);

		[HttpPost]
		[Route("DoPostImportProcessingAsync")]
		Task DoPostImportProcessingAsync(int workspaceID, int productionArtifactID, string correlationID);

		[HttpPost]
		[Route("DoPreImportProcessingAsync")]
		Task DoPreImportProcessingAsync(int workspaceID, int productionArtifactID, string correlationID);

		[HttpPost]
		[Route("ReadAsync")]
		Task<ProductionInfo> ReadAsync(int workspaceID, int productionArtifactID, string correlationID);

		[HttpPost]
		[Route("ReadWithoutValidationAsync")]
		Task<ProductionInfo> ReadWithoutValidationAsync(int workspaceID, int productionArtifactID, string correlationID);
	}
}