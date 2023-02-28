using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry;
using kCura.Utility;
using kCura.Utility.Extensions;
using Microsoft.SqlServer.Server;
using Relativity.Core;
using Relativity.Core.Service;
using Relativity.Logging;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.Core
{
	internal class ServerSideFolderGenerator
	{
		public void CreateFolders(ILockHelper lockHelper, Timekeeper timekeeper, BaseContext context, int activeWorkspaceID, Folder folder, DTO.NativeLoadInfo settings, ILog logger)
		{
			// If settings.RootFolderID is 0, the folders have been created with an old version of the client. New clients pass -1 for unset and > 0 for set, which does not create folders on the client.
			// Check here to see whether the settings object has a Root Folder ID less than zero.
			// 0 = old client, built folders on the client, MassImportManager will not attempt to construct them
			// < 0 = new client, Folder ID not set in Load File (some UI's allow for this), Should determine the workspace's root folder
			// > 0 = new client, Folder ID set to an existing folder, any newly created folders will all be beneath this folder
			if (settings.RootFolderID < 0)
			{
				settings.RootFolderID = Relativity.Core.Service.SystemArtifactQuery.Instance()
					.RetrieveArtifactIDByIdentifier(context, Relativity.Core.SystemArtifact.RootFolder.ToString());
			}

			lockHelper.Lock(context, MassImportManagerLockKey.LockType.Folder, () =>
			{
				timekeeper.MarkStart("CreateFolders");

				try
				{
					// We grab the folders we need to create by pulling back the folders with id == -9. 
					System.Data.DataTable folderPaths = folder.GetFolderPathsForFoldersWithoutIDs(activeWorkspaceID);
					if (folderPaths.Rows.Count > 0)
					{
						kCura.Data.DataView kCuraDataView = new kCura.Data.DataView(folderPaths);

						FolderNode rootNode = new FolderNode();

						// Clean up the folder paths to ensure they don't have invalid characters
						// Add them to a reader
						// And prepare the folder tree
						foreach (System.Data.DataRowView dataRow in kCuraDataView)
						{
							int identity = System.Convert.ToInt32(dataRow["kCura_Import_ID"]);
							string path = System.Convert.ToString(dataRow["kCura_Import_ParentFolderPath"]);
							string friendlyFolderName = FolderManager.GetExportFriendlyFolderName(path);

							rootNode.Add(friendlyFolderName, identity);
						}

						// Create missing folders
						List<FolderNode> folderNodes = rootNode.Descendants().ToList();
						if (folderNodes.Count > 0)
						{
							IEnumerable<SqlDataRecord> folderCandidates = folderNodes.GetFolderCandidates();

							List<FolderArtifactIDMapping> folderArtifactIdMappings =
								folder.CreateMissingFolders(folderCandidates, settings.RootFolderID,
									rootNode.TempArtifactID, context.UserID);

							// The query inside the CreateMassAuditRecords uses IN clause. Just to be safe, we batch the requests to avoid the limitations of the IN clause
							const Int32 BATCH_SIZE = 2000;
							foreach (IEnumerable<Int32> createdArtifactIDsBatch in folderArtifactIdMappings
								.Where(p => p.NewFolder).Select(p => p.ArtifactID).Divide(BATCH_SIZE))
							{
								Relativity.Core.AuditHelper.CreateMassAuditRecords(context,
									createdArtifactIDsBatch.ToArray(), (int) Relativity.Core.AuditAction.Create,
									"''", string.Empty);
							}

							try
							{
								IEnumerable<SqlDataRecord> importMap = folderNodes.GetImportMapping(folderArtifactIdMappings);
								folder.SetParentFolderIDsToRootFolderID(importMap, activeWorkspaceID, settings.RootFolderID);
							}
							catch (Exception ex)
							{
								logger.LogError(ex, "Failed to CreateFolders folderNodesCount: {nodesCount} folderCandidatesCount: {candidatesCount} folderArtifactIdMappingsCount: {mappingsCount}",
									folderNodes.Count, folderCandidates.Count(), folderArtifactIdMappings.Count);

								TraceHelper.SetStatusError(Activity.Current, $"Failed to CreateFolders folderNodesCount: {folderNodes.Count} folderCandidatesCount: {folderCandidates.Count()} folderArtifactIdMappingsCount: {folderArtifactIdMappings.Count}: {ex.Message}", ex);

								logger.LogError("folderArtifactIdMappings: {@mappings}", folderArtifactIdMappings);
								foreach (var folderArtifactIdMapping in folderArtifactIdMappings)
								{
									logger.LogError("folderArtifactIdMapping: {@mapping}", folderArtifactIdMapping);
								}
								foreach (var folderNode in folderNodes)
								{
									logger.LogError("folderNode: {@node}", new { TempArtifactID = folderNode.TempArtifactID, LeafsCount = folderNode.LeafIDs.Count, Leafs = string.Join(",", folderNode.LeafIDs) });
								}

								throw;
							}
						}
						else
						{
							folder.SetParentFolderIDsToRootFolderID(activeWorkspaceID, settings.RootFolderID);
						}
					}
				}
				catch (Exception ex) when ((ex.InnerException as SqlException)?.Number == 50005)
				{
					Match match = Regex.Match(ex.InnerException.Message, @"Permission\|(View|Add)\|(\d+)");

					if (match.Success)
					{
						Relativity.Core.Exception.Permission.Type exceptionType;
						Enum.TryParse(match.Groups[1].Value, out exceptionType);
						throw new Relativity.Core.Exception.Permission(exceptionType,
							int.Parse(match.Groups[2].Value), context);
					}
				}

				timekeeper.MarkEnd("CreateFolders");
			});
		}
	}
}