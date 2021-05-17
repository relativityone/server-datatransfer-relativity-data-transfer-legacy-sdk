using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("Bulk Import Manager replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("bulk-import")]
	public interface IBulkImportService : IDisposable
	{
		[HttpPost]
		Task<MassImportResults> BulkImportImageAsync(int workspaceID, ImageLoadInfo settings, bool inRepository, string correlationID);

		[HttpPost]
		Task<MassImportResults> BulkImportProductionImageAsync(int workspaceID, ImageLoadInfo settings, int productionArtifactID, bool inRepository, string correlationID);

		[HttpPost]
		Task<MassImportResults> BulkImportNativeAsync(int workspaceID, NativeLoadInfo settings, bool inRepository, bool includeExtractedTextEncoding, string correlationID);

		[HttpPost]
		Task<MassImportResults> BulkImportObjectsAsync(int workspaceID, ObjectLoadInfo settings, bool inRepository, string correlationID);

		[HttpPost]
		Task<ErrorFileKey> GenerateImageErrorFilesAsync(int workspaceID, string importKey, bool writeHeader, int keyFieldID, string correlationID);

		[HttpPost]
		Task<bool> ImageRunHasErrorsAsync(int workspaceID, string runID, string correlationID);

		[HttpPost]
		Task<ErrorFileKey> GenerateNonImageErrorFilesAsync(int workspaceID, string importKey, int artifactTypeID, bool writeHeader, int keyFieldID, string correlationID);

		[HttpPost]
		Task<bool> NativeRunHasErrorsAsync(int workspaceID, string runID, string correlationID);

		[HttpPost]
		Task<object> DisposeTempTablesAsync(int workspaceID, string runID, string correlationID);

		[HttpPost]
		Task<bool> HasImportPermissionsAsync(int workspaceID, string correlationID);
	}
}