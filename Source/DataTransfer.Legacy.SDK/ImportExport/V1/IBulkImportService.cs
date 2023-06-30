using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	using System.Collections.Generic;

	[WebService("Bulk Import Manager replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("bulk-import")]
	public interface IBulkImportService : IDisposable
	{
		[HttpPost]
		[Route("BulkImportImageAsync")]
		Task<MassImportResults> BulkImportImageAsync(int workspaceID, [ImageLoadInfoData] ImageLoadInfo settings, bool inRepository, string correlationID);

		[HttpPost]
		[Route("BulkImportProductionImageAsync")]
		Task<MassImportResults> BulkImportProductionImageAsync(int workspaceID, [ImageLoadInfoData] ImageLoadInfo settings, int productionArtifactID, bool inRepository, string correlationID);

		[HttpPost]
		[Route("BulkImportNativeAsync")]
		Task<MassImportResults> BulkImportNativeAsync(int workspaceID, [NativeLoadInfoData] NativeLoadInfo settings, bool inRepository, bool includeExtractedTextEncoding, string correlationID);

		[HttpPost]
		[Route("BulkImportObjectsAsync")]
		Task<MassImportResults> BulkImportObjectsAsync(int workspaceID, [ObjectLoadInfoData] ObjectLoadInfo settings, bool inRepository, string correlationID);

		[HttpPost]
		[Route("GenerateImageErrorFilesAsync")]
		Task<ErrorFileKey> GenerateImageErrorFilesAsync(int workspaceID, string importKey, bool writeHeader, int keyFieldID, string correlationID);

		[HttpPost]
		[Route("ImageRunHasErrorsAsync")]
		Task<bool> ImageRunHasErrorsAsync(int workspaceID, string runID, string correlationID);

		[HttpPost]
		[Route("GenerateNonImageErrorFilesAsync")]
		Task<ErrorFileKey> GenerateNonImageErrorFilesAsync(int workspaceID, string importKey, int artifactTypeID, bool writeHeader, int keyFieldID, string correlationID);

		[HttpPost]
		[Route("NativeRunHasErrorsAsync")]
		Task<bool> NativeRunHasErrorsAsync(int workspaceID, string runID, string correlationID);

		[HttpPost]
		[Route("DisposeTempTablesAsync")]
		Task<object> DisposeTempTablesAsync(int workspaceID, string runID, string correlationID);

		[HttpPost]
		[Route("HasImportPermissionsAsync")]
		Task<bool> HasImportPermissionsAsync(int workspaceID, string correlationID);

		[HttpPost]
		[Route("GetBulkImportResultAsync")]
		Task<MassImportResults> GetBulkImportResultAsync(int workspaceID, string runID, string correlationID);

		[HttpPost]
		[Route("GetIdentifiersOfImportedDocumentsAsync")]
		Task<List<string>> GetIdentifiersOfImportedDocumentsAsync(int workspaceID, string runID, int keyFieldID, string correlationID);

		[HttpPost]
		[Route("GetIdentifiersOfImportedImagesAsync")]
		Task<List<string>> GetIdentifiersOfImportedImagesAsync(int workspaceID, string runID, string correlationID);

		[HttpPost]
		[Route("NativeRunHasErrorsDoNotTruncateAsync")]
		Task<bool> NativeRunHasErrorsDoNotTruncateAsync(int workspaceID, string runID, string correlationID);

		[HttpPost]
		[Route("ImageRunHasErrorsDoNotTruncateAsync")]
		Task<bool> ImageRunHasErrorsDoNotTruncateAsync(int workspaceID, string runID, string correlationID);

		[HttpPost]
		[Route("GenerateNonImageErrorFilesDoNotTruncateAsync")]
		Task<ErrorFileKey> GenerateNonImageErrorFilesDoNotTruncateAsync(int workspaceID, string importKey, int artifactTypeID, bool writeHeader, int keyFieldID, string correlationID);


		[HttpPost]
		[Route("GenerateImageErrorFilesDoNotTruncateAsync")]
		Task<ErrorFileKey> GenerateImageErrorFilesDoNotTruncateAsync(int workspaceID, string runID, bool writeHeader, int keyFieldID, string correlationID);

	}
}