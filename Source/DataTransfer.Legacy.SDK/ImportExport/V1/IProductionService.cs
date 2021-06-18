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
		Task<ExportDataWrapper> RetrieveBatesByProductionAndDocumentAsync(int workspaceID, int[] productionIDs, int[] documentIDs, string correlationID);

		[HttpPost]
		Task<DataSetWrapper> RetrieveProducedByContextArtifactIDAsync(int workspaceID, string correlationID);

		[HttpPost]
		Task<DataSetWrapper> RetrieveImportEligibleByContextArtifactIDAsync(int workspaceID, string correlationID);

		[HttpPost]
		Task DoPostImportProcessingAsync(int workspaceID, int productionArtifactID, string correlationID);

		[HttpPost]
		Task DoPreImportProcessingAsync(int workspaceID, int productionArtifactID, string correlationID);

		[HttpPost]
		Task<ProductionInfo> ReadAsync(int workspaceID, int productionArtifactID, string correlationID);
	}
}