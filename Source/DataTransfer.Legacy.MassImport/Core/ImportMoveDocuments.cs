using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataTransfer.Legacy.MassImport.Data.Cache;

namespace Relativity.MassImport.Core
{
	internal class ImportMoveDocuments
	{
		private Relativity.Core.BaseContext _context;
		private Relativity.Core.MassPermissionManager _massDeletePermissionManager;
		private Relativity.Core.DTO.Folder _destinationFolderDTO;
		private string _destinationFullPath;
		private Dictionary<int, string> _artifactFullPathMap = new Dictionary<int, string>();
		private Dictionary<int, int> _artifactToParentMap = new Dictionary<int, int>();
		private string _metadataNativeJoinTableName;

		#region Constructors
		public ImportMoveDocuments(Relativity.Core.BaseContext context) : base()
		{
			// Validate arguments on public methods
			if (context is null)
			{
				throw new ArgumentNullException("context");
			}

			_context = context;
			_massDeletePermissionManager = new Relativity.Core.MassPermissionManager(_context, (int) ArtifactType.Document, Relativity.Data.PermissionHelper.Type.Delete);
		}
		#endregion

		#region Execute methods
		public void Execute(string runId, string tableNameNativeTemp)
		{
			try
			{
				var destinationFoldersFromMetadata = this.GetNewDestinationFoldersFromMetadata(_context.DBContext, runId, tableNameNativeTemp);

				foreach (DocumentsGroup row in destinationFoldersFromMetadata)
				{
					int destinationArtifactID = row.DestinationArtifactId;
					var artifactIDs = row.ArtifactIDs;
					ExecuteOnDocument(destinationArtifactID, artifactIDs);
				}

				DropTempJoinTable();
			}
			catch (Relativity.Core.Exception.BulkOperationPermissionException)
			{
				throw;
			}
		}

		private void ExecuteOnDocument(int rootId, IEnumerable<int> artifactIDs)
		{
			// Setup destination container information
			_destinationFolderDTO = Relativity.Core.Service.FolderManager.Read(_context, rootId);

			var destData = new DestinationData(_context, rootId);
			_destinationFullPath = GetAndCacheFullPath(rootId);

			var batcher = new Relativity.Core.IdListMassProcessBatch(artifactIDs.ToArray(), Relativity.Data.Config.MassMoveBatchAmount);
			var artifactIDsBatch = batcher.GetNextBatch();
			while (artifactIDsBatch is object)
			{
				this.PreLoadFullPathCache(artifactIDsBatch);

				this.MoveDocuments(artifactIDsBatch, destData, this.CreateAuditMessagesForArtifactList(artifactIDsBatch));

				artifactIDsBatch = batcher.GetNextBatch();
			}
		}
		#endregion

		#region Move methods
		private IEnumerable<DocumentsGroup> GetNewDestinationFoldersFromMetadata(kCura.Data.RowDataGateway.BaseContext context, string runId, string tableNameNativeTemp)
		{
			_metadataNativeJoinTableName = GetMetadataNativeJoinTableName(runId);

			// create the join table
			string createJoinTableSql = GetCreateJoinTableSqlClause();
			context.ExecuteNonQuerySQLStatement(createJoinTableSql);

			// Update the join metadata table with temp document
			string updateJoinTableSql = GetJoinTableSqlClause(tableNameNativeTemp);
			context.ExecuteNonQuerySQLStatement(updateJoinTableSql);

			string sqlText = $"SELECT * FROM [Resource].[{_metadataNativeJoinTableName}]";

			var documentsWithRequiredFolderPathUpdate = context.ExecuteSqlStatementAsDataTable(sqlText);

			var documentsGroupedByParentFolderId = from row in documentsWithRequiredFolderPathUpdate.AsEnumerable()
												   group row by row.Field<int>("MetadataParentArtifactID") into DestinationFolderArtifactGroup
												   let ParentArtifactID = DestinationFolderArtifactGroup.Key
												   select new DocumentsGroup(ParentArtifactID, DestinationFolderArtifactGroup.Select(r => r.Field<int>("ArtifactID")));
			return documentsGroupedByParentFolderId;
		}

		private string GetCreateJoinTableSqlClause()
		{
			return $@"IF NOT EXISTS(SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{_metadataNativeJoinTableName}')
						CREATE TABLE [Resource].[{_metadataNativeJoinTableName}](
							[ArtifactID] [int] NOT NULL,
							[ParentArtifactID] [int] NOT NULL,
							[MetadataParentArtifactID] [int] NOT NULL

						)";
		}

		private string GetJoinTableSqlClause(string tableNameNativeTemp)
		{
			return $@"INSERT INTO [Resource].[{_metadataNativeJoinTableName}] (ArtifactID, ParentArtifactID,MetadataParentArtifactID)
					SELECT D.[ArtifactID]
						,D.[ParentArtifactID_D]
						,M.[kCura_Import_ParentFolderId]
						FROM [Document] D
						INNER JOIN [Resource].[{tableNameNativeTemp}] M
						On D.ArtifactID = M.ArtifactID
						Where D.[ParentArtifactID_D] != M.[kCura_Import_ParentFolderId]";
		}

		private string GetDropJoinTableSqlClause()
		{
			return $@"IF EXISTS (SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{_metadataNativeJoinTableName}')
							BEGIN
								DROP TABLE [Resource].[{_metadataNativeJoinTableName}]
							END";
		}

		private string GetMetadataNativeJoinTableName(string runId)
		{
			// temp table name
			return string.Concat("NAT_METADATA_JOIN_", runId);
		}

		private void MoveDocuments(int[] artifactIDs, DestinationData destData, Dictionary<string, List<int>> auditMessages)
		{
			if (!_massDeletePermissionManager.HasPermission(artifactIDs))
			{
				throw new Relativity.Core.Exception.BulkOperationPermissionException(Relativity.Data.PermissionHelper.Type.Delete);
			}

			this.UpdateArtifactTablesForMoveByMultipleArtifactIDs(_context, artifactIDs, destData.DestinationArtifactID, _destinationFolderDTO.AccessControlListID, auditMessages);
		}

		private void UpdateArtifactTablesForMoveByMultipleArtifactIDs(Relativity.Core.BaseContext context, int[] artifactIDs, int parentArtifactID, int accessControlListID, Dictionary<string, List<int>> auditStrings)
		{
			var docManager = new Relativity.Core.Service.DocumentManager();

			int queryTimeOut = kCura.Data.RowDataGateway.Config.LongRunningQueryTimeout;

			docManager.UpdateParentageByMultipleArtifactIDs(context, artifactIDs, parentArtifactID, accessControlListID, queryTimeOut);

			Relativity.Data.ArtifactQuery.UpdateParentageByMultipleArtifactIDs(context.DBContext, artifactIDs, parentArtifactID, accessControlListID, context.UserID, queryTimeOut);

			Relativity.Data.ArtifactQuery.UpdateAncestryByMultipleArtifactIDs(context.DBContext, artifactIDs, parentArtifactID);

			foreach (string item in auditStrings.Keys)
			{
				Relativity.Core.AuditHelper.CreateMassAuditRecords(context.ChicagoContext, auditStrings[item], (int) Relativity.Core.AuditAction.Move, "'" + item + "'", string.Empty);
			}
		}

		private int DropTempJoinTable()
		{
			return _context.DBContext.ExecuteNonQuerySQLStatement(GetDropJoinTableSqlClause(), InstanceSettings.MassImportSqlTimeout);
		}
		#endregion

		#region Path Cache methods
		public void PreLoadFullPathCache(int[] artifactIDs)
		{
			// Validate arguments on public methods
			if (artifactIDs is null)
			{
				throw new ArgumentNullException("artifactIDs");
			}

			// Retrieve all of the parent Ids for the given artifacts
			var artifactParentIds = Relativity.Core.Query.Artifact.RetrieveArtifactIDsWithTheirParentArtifactIDs(_context, artifactIDs);

			// Traverse the list of parent Ids and add their full hierarchical
			// path to our cache map.
			foreach (DataRowView item in artifactParentIds)
			{
				int artifactId = (int) item["ArtifactID"];
				int parentArtifactId = (int) item["ParentArtifactID"];

				_artifactToParentMap[artifactId] = parentArtifactId;

				if (!_artifactFullPathMap.ContainsKey(parentArtifactId))
				{
					GetAndCacheFullPath(parentArtifactId);
				}
			}
		}

		private string GetAndCacheFullPath(int rootId)
		{
			if (_artifactFullPathMap.ContainsKey(rootId))
			{
				return _artifactFullPathMap[rootId];
			}
			else
			{
				string artifactFullPath;
				artifactFullPath = Relativity.Data.Folder.GetFolderPath(_context.ChicagoContext.DBContext, rootId);
				_artifactFullPathMap.Add(rootId, artifactFullPath);

				return artifactFullPath;
			}
		}
		#endregion

		#region Audit methods
		private Dictionary<string, List<int>> CreateAuditMessagesForArtifactList(int[] artifactIDs)
		{
			var auditMessages = new Dictionary<string, List<int>>();
			if (Relativity.Core.Config.AuditingEnabled)
			{
				foreach (int item in artifactIDs)
				{
					string auditMessage = CreateAuditMessageForArtifact(item);
					List<int> auditArtifactIds = null;
					if (!auditMessages.TryGetValue(auditMessage, out auditArtifactIds))
					{
						auditMessages.Add(auditMessage, new List<int>() { item });
					}
					else
					{
						auditArtifactIds.Add(item);
					}
				}
			}

			return auditMessages;
		}

		private string CreateAuditMessageForArtifact(int documentArtifactId)
		{
			string sourceFullPath;

			int sourceFolderArtifactId = _artifactToParentMap[documentArtifactId];
			sourceFullPath = GetAndCacheFullPath(sourceFolderArtifactId);

			return Relativity.Core.MoveHelper.CreateAuditMessageForArtifact(sourceFolderArtifactId.ToString(), sourceFullPath, _destinationFolderDTO.ArtifactID, _destinationFullPath);
		}
		#endregion

		#region DestinationData class
		public class DestinationData
		{
			private int _destinationArtifactID;
			private ArrayList _newAncestorArtifactIDs = new ArrayList();

			public ArrayList NewAncestorArtifactIDs
			{
				get
				{
					return _newAncestorArtifactIDs;
				}
			}

			public int DestinationArtifactID
			{
				get
				{
					return _destinationArtifactID;
				}
			}

			public DestinationData(Relativity.Core.ICoreContext sc, int destinationArtifactID)
			{
				// Validate arguments on public methods
				if (sc is null)
				{
					throw new ArgumentNullException("sc");
				}

				_destinationArtifactID = destinationArtifactID;
				_newAncestorArtifactIDs.Add(_destinationArtifactID);

				var destinationAncestryItems = Relativity.Core.Query.Artifact.RetrieveAncestorArtifactIDsWithDepthByArtifactID(sc, _destinationArtifactID);

				foreach (DataRowView item in destinationAncestryItems)
				{
					int ancestorArtifactID = (int) item["AncestorArtifactID"];
					_newAncestorArtifactIDs.Add(ancestorArtifactID);
				}
			}
		}
		#endregion
	}

	public class DocumentsGroup
	{
		public DocumentsGroup(int destinationArtifactId, IEnumerable<int> artifactIDs)
		{
			DestinationArtifactId = destinationArtifactId;
			ArtifactIDs = artifactIDs;
		}

		public int DestinationArtifactId { get; private set; }
		public IEnumerable<int> ArtifactIDs { get; private set; }
	}
}