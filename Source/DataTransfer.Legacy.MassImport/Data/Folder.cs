using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using Relativity.Data.MassImport;
using DataTransfer.Legacy.MassImport.Data.Cache;
using DataTransfer.Legacy.MassImport.Toggles;
using Relativity.Toggles;

namespace Relativity.MassImport.Data
{
	internal class Folder
	{
		private readonly kCura.Data.RowDataGateway.BaseContext _context;
		private readonly TableNames _tableNames;
		private readonly int _queryTimeout;
		private const string CreateMissingFolders_21f65fdc_3016_4f2b_9698_de151a6186a2 = "CreateMissingFolders_21f65fdc-3016-4f2b-9698-de151a6186a2";
		private const string FolderCandidateTableType_21f65fdc_3016_4f2b_9698_de151a6186a2 = "FolderCandidateTableType_21f65fdc-3016-4f2b-9698-de151a6186a2";
		private const string CreateMissingFoldersName = "CreateMissingFolders";
		private const string FolderCandidateTableTypeName = "FolderCandidateTableType";

		public Folder(kCura.Data.RowDataGateway.BaseContext context, TableNames tableNames)
		{
			_context = context;
			_tableNames = tableNames;
			_queryTimeout = InstanceSettings.MassImportSqlTimeout;
		}

		public DataTable GetFolderPathsForFoldersWithoutIDs(int workspaceID)
		{
			DataTable missingFolderIDsFromBulkTable;
			string bulkTableName = $"[EDDS{workspaceID}].[Resource].[{_tableNames.Native}]";
			// DAG965 Because of the way the RELNATTMP table was created, it ends up with '\' if the initial folder path was empty or null
			// We need to replace this value with '' because '\' it would eventually cause WebAPI to create a child folder named '' at the root level.
			string sqlStatement = $@"
IF OBJECT_ID('{bulkTableName}','TABLE') IS NOT NULL
BEGIN
  SELECT [kCura_Import_ID], (CASE WHEN [kCura_Import_ParentFolderPath] = '\' THEN '' ELSE [kCura_Import_ParentFolderPath] END) AS [kCura_Import_ParentFolderPath]
	FROM {bulkTableName}
	WHERE [kCura_Import_ParentFolderID] = -9
END
";
			missingFolderIDsFromBulkTable = _context.ExecuteSqlStatementAsDataTable(sqlStatement, _queryTimeout);
			return missingFolderIDsFromBulkTable;
		}

		public List<FolderArtifactIDMapping> CreateMissingFolders(IEnumerable<SqlDataRecord> candidate, int rootFolderID, int tempRootFolderID, int userID)
		{
			try
			{
				if (ToggleProvider.Current.IsEnabled<EnableNonLatinCharFolderErrorFreeCreateNewFolderToggle>())
				{
					return CreateMissingFoldersRunStoreProcedure(CreateMissingFolders_21f65fdc_3016_4f2b_9698_de151a6186a2, FolderCandidateTableType_21f65fdc_3016_4f2b_9698_de151a6186a2, candidate, rootFolderID, tempRootFolderID, userID);
				}

				return CreateMissingFoldersRunStoreProcedure(CreateMissingFoldersName, FolderCandidateTableTypeName, candidate, rootFolderID, tempRootFolderID, userID);
			}
			catch (Exception ex)
			{
				if (ex.InnerException.Message == "Could not find stored procedure 'CreateMissingFolders_21f65fdc-3016-4f2b-9698-de151a6186a2'.")
				{
					return CreateMissingFoldersRunStoreProcedure(CreateMissingFoldersName, FolderCandidateTableTypeName, candidate, rootFolderID, tempRootFolderID, userID);
				}

				throw;
			}
		}

		public void SetParentFolderIDsToRootFolderID(int workspaceID, int rootFolderID)
		{
			string bulkTableName = string.Format("[EDDS{0}].[Resource].[{1}]", (object)workspaceID, _tableNames.Native);
			string sqlStatement = $@"
					IF OBJECT_ID('{ bulkTableName }','TABLE') IS NOT NULL
					BEGIN
						UPDATE { bulkTableName } SET [kCura_Import_ParentFolderID] = { rootFolderID }
						WHERE [kCura_Import_ParentFolderID] = -9
					END
";
			_context.ExecuteNonQuerySQLStatement(sqlStatement, _queryTimeout);
		}

		public void SetParentFolderIDsToRootFolderID(IEnumerable<SqlDataRecord> importIDMap, int workspaceID, int rootFolderID)
		{
			string bulkTableName = string.Format("[EDDS{0}].[Resource].[{1}]", (object)workspaceID, _tableNames.Native);
			string sqlStatement = $@"
					IF OBJECT_ID('{ bulkTableName }','TABLE') IS NOT NULL
					BEGIN
						UPDATE N SET [kCura_Import_ParentFolderID] = COALESCE(M.[ParentArtifactID], @rootFolderID)
							FROM { bulkTableName } N
								LEFT JOIN @importIDMap M ON M.[ImportID] = N.[kCura_Import_ID]
								WHERE M.[ImportID] IS NOT NULL OR (M.[ImportID] IS NULL AND N.[kCura_Import_ParentFolderID] = -9)
					END
";
			var parameterList = new List<SqlParameter>();

			var importIDMapParameter = new SqlParameter("@importIDMap", importIDMap);
			importIDMapParameter.SqlDbType = SqlDbType.Structured;
			importIDMapParameter.TypeName = "EDDSDBO.FolderImportIDMapTableType";
			parameterList.Add(importIDMapParameter);

			var rootFolderIDParameter = new SqlParameter("@rootFolderID", rootFolderID);
			rootFolderIDParameter.SqlDbType = SqlDbType.Int;
			parameterList.Add(rootFolderIDParameter);

			_context.ExecuteNonQuerySQLStatement(sqlStatement, parameterList, _queryTimeout);
		}

		private List<FolderArtifactIDMapping> CreateMissingFoldersRunStoreProcedure(string storeProcedureName, string typeName, IEnumerable<SqlDataRecord> candidate, int rootFolderID, int tempRootFolderID, int userID)
		{
			var parameterList = new List<SqlParameter>();

			var candidateParameter = new SqlParameter("@candidate", candidate);
			candidateParameter.SqlDbType = SqlDbType.Structured;
			candidateParameter.TypeName = $"EDDSDBO.{typeName}";
			parameterList.Add(candidateParameter);

			var rootFolderIDParameter = new SqlParameter("@rootFolderID", rootFolderID);
			rootFolderIDParameter.SqlDbType = SqlDbType.Int;
			parameterList.Add(rootFolderIDParameter);

			var tempRootFolderIDParameter = new SqlParameter("@tempRootFolderID", tempRootFolderID);
			tempRootFolderIDParameter.SqlDbType = SqlDbType.Int;
			parameterList.Add(tempRootFolderIDParameter);

			var userIDParameter = new SqlParameter("@userID", userID);
			userIDParameter.SqlDbType = SqlDbType.Int;
			parameterList.Add(userIDParameter);

			var folderArtifactIdMappings = new List<FolderArtifactIDMapping>();
			using (var reader = _context.ExecuteProcedureAsReader(storeProcedureName, parameterList))
			{
				while (reader.Read())
				{
					folderArtifactIdMappings.Add(new FolderArtifactIDMapping(Convert.ToInt32(reader[0]), Convert.ToInt32(reader[1]), Convert.ToBoolean(reader[2])));
				}
			}

			return folderArtifactIdMappings;
		}
	}
}