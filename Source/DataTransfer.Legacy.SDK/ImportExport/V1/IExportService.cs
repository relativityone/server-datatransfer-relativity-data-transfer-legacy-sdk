﻿using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("Export Manager replacement Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("export")]
	public interface IExportService : IDisposable
	{
		[HttpPost]
		[Route("InitializeSearchExportAsync")]
		Task<InitializationResults> InitializeSearchExportAsync(int workspaceID, int searchArtifactID, int[] avfIDs, int startAtRecord, string correlationID);

		[HttpPost]
		[Route("InitializeFolderExportAsync")]
		Task<InitializationResults> InitializeFolderExportAsync(int workspaceID, int viewArtifactID, int parentArtifactID, bool includeSubFolders, int[] avfIDs, int startAtRecord, int artifactTypeID, string correlationID);

		[HttpPost]
		[Route("InitializeProductionExportAsync")]
		Task<InitializationResults> InitializeProductionExportAsync(int workspaceID, int productionArtifactID, int[] avfIds, int startAtRecord, string correlationID);

		/// <summary>
		/// this really returns array of objects
		/// </summary>
		[HttpPost]
		[Route("RetrieveResultsBlockForProductionStartingFromIndexAsync")]
		Task<ExportDataWrapper> RetrieveResultsBlockForProductionStartingFromIndexAsync(int workspaceID, Guid runID, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter,
			int[] textPrecedenceAvfIds, int productionId, int index, string correlationID);

		/// <summary>
		/// this really returns array of objects
		/// </summary>
		[HttpPost]
		[Route("RetrieveResultsBlockStartingFromIndexAsync")]
		Task<ExportDataWrapper> RetrieveResultsBlockStartingFromIndexAsync(int workspaceID, Guid runID, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter,
			int[] textPrecedenceAvfIds, int index, string correlationID);

		[HttpPost]
		[Route("HasExportPermissionsAsync")]
		Task<bool> HasExportPermissionsAsync(int workspaceID, string correlationID);
	}
}