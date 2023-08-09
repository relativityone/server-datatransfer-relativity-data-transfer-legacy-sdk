using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Relativity.MassImport.Data.Cache;
using Relativity.Data.MassImport;

namespace Relativity.MassImport.Data
{
	internal class Helper
	{
		public static string GenerateAuditInsertClause(int auditActionID, int userID, string requestOrigination, string recordOrigination, string artifactIdSourceTable)
		{
			return GenerateAuditInsertClause(auditActionID, userID, requestOrigination, recordOrigination, artifactIdSourceTable, -1, -1);
		}

		public static string GenerateAuditInsertClause(int auditActionID, int userID, string requestOrigination, string recordOrigination, string artifactIdSourceTable, int start, int count)
		{
			string fromLocation;
			if (count > 0)
			{
				fromLocation = string.Format(" ( SELECT TOP {0} FROM [Resource].[{1}] ) A", count, artifactIdSourceTable);
			}
			else
			{
				fromLocation = string.Format("[Resource].[{0}]", artifactIdSourceTable);
			}

			string sql = $@"
INSERT INTO AuditRecord (
	[ArtifactID],
	[Action],
	[Details],
	[UserID],
	[TimeStamp],
	[RequestOrigination],
	[RecordOrigination]
)	SELECT DISTINCT
	[ArtifactID],
	{ auditActionID },
	'',
	{ userID },
	GETUTCDATE(),
	'{ requestOrigination.Replace("'", "''").Replace("[", "[[") }',
	'{ recordOrigination.Replace("'", "''").Replace("[", "[[") }'
FROM
	{ fromLocation }";

			return sql;
		}

		public static string GenerateOutputDeletedIntoClause(int auditActionID, int userID, string requestOrigination, string recordOrigination)
		{
			string sql = $@"
	OUTPUT
		Deleted.[DocumentArtifactID] 'OutputArtifactID',
		{auditActionID} 'OutputAction',
		'' 'OutputDetails',
		{userID} 'OutputUserID',
		GETUTCDATE() 'OutputTimeStamp',
		'{requestOrigination.Replace("'", "''").Replace("[", "[[")}' 'OutputRequestOrigination',
		'{recordOrigination.Replace("'", "''").Replace("[", "[[")}' 'OutputRecordOrigination'
";
			return sql;
		}

		public static void DropRunTempTables(kCura.Data.RowDataGateway.BaseContext context, string runId)
		{
			if (string.IsNullOrWhiteSpace(runId))
				return;
			try
			{
				var x = new Guid(runId.Replace("_", ""));
			}
			catch (Exception)
			{
				throw new Exception("Invalid run ID");
			}

			var statements = from tableName in TableNames.GetAllTempTableNames(runId)
					.Concat(TableNames.GetAllAuxiliaryTableNames(runId))
				select
					$@"IF EXISTS (SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{tableName}')
			BEGIN
				DROP TABLE[Resource].[{tableName}]
			END ";

			string combinedStatement = string.Join(Environment.NewLine, statements);
			context.ExecuteNonQuerySQLStatement(combinedStatement, Relativity.Data.Config.MassImportSqlTimeout);
		}

		public static Hashtable GetFieldCollationLookup(kCura.Data.RowDataGateway.BaseContext context, string artifactTypeTableName)
		{
			var retval = new Hashtable();
			foreach (DataRow row in context.ExecuteSqlStatementAsDataTable(string.Format("SELECT Field.ArtifactID, collation_name FROM sys.columns INNER JOIN ArtifactViewField ON ArtifactViewField.ColumnName COLLATE SQL_Latin1_General_CP1_CI_AS = sys.columns.[name] COLLATE SQL_Latin1_General_CP1_CI_AS  INNER JOIN [Field] ON Field.ArtifactViewFieldID = ArtifactViewField.ArtifactViewFieldID WHERE [object_id] = OBJECT_ID(N'eddsdbo.{0}', N'U') AND NOT collation_name IS NULL", artifactTypeTableName)).Rows)
				retval.Add(Convert.ToInt32(row["ArtifactID"]), row["collation_name"].ToString());
			return retval;
		}

		internal static List<FieldInfo> GetFieldsForArtifactTypeByCategory(kCura.Data.RowDataGateway.BaseContext context, int artifactTypeID, FieldCategory category)
		{
			string sqlQuery = "SELECT ArtifactID, FieldCategoryID, FieldTypeID, CodeTypeID, DisplayName, MaxLength, ImportBehavior, EnableDataGrid FROM [Field] WHERE FieldCategoryID = @fieldCategory AND FieldArtifactTypeID = @artifactType";
			var parameters = new[] { new System.Data.SqlClient.SqlParameter("@artifactType", artifactTypeID), new System.Data.SqlClient.SqlParameter("@fieldCategory", Convert.ToInt32(category)) };
			var retval = context.ExecuteSqlStatementAsDataTable(sqlQuery, parameters, Relativity.Data.Config.MassImportSqlTimeout)
				.Select()
				.Select(row => CreateFieldInfo(row))
				.ToList();
			return retval;
		}

		internal static FieldInfo CreateFieldInfo(DataRow row)
		{
			var field = new FieldInfo();
			field.ArtifactID = Convert.ToInt32(row["ArtifactID"]);
			field.Category = (FieldCategory) Convert.ToInt32(row["FieldCategoryID"]);
			field.DisplayName = row["DisplayName"].ToString();
			field.Type = (FieldTypeHelper.FieldType) Convert.ToInt32(row["FieldTypeID"]);
			if (field.Type == FieldTypeHelper.FieldType.Varchar)
			{
				field.TextLength = Convert.ToInt32(row["MaxLength"]);
			}

			if (field.Type == FieldTypeHelper.FieldType.Code || field.Type == FieldTypeHelper.FieldType.MultiCode)
			{
				field.CodeTypeID = Convert.ToInt32(row["CodeTypeID"]);
			}

			field.ImportBehavior = kCura.Utility.NullableTypesHelper.DBNullConvertToNullable<FieldInfo.ImportBehaviorChoice>(row["ImportBehavior"]);
			field.EnableDataGrid = Convert.ToBoolean(row["EnableDataGrid"]);
			return field;
		}

		/// <summary>
		/// A return value of "1", or a SQL statement that evaluates to "1", means the CodeArtifact values will be merged/appended.
		/// A return value of "0", or a SQL statement that evaluates to "0", means that CodeArtifact values will be replaced.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="fieldType"></param>
		/// <param name="overlayMergeValues"></param>
		/// <returns>a SQL expression that will be used when modifying the CodeArtifact table</returns>
		/// <remarks></remarks>
		public static string GetFieldOverlaySwitchStatement(Relativity.MassImport.DTO.NativeLoadInfo settings, FieldTypeHelper.FieldType fieldType, bool? overlayMergeValues)
		{
			// This function is called only for multi choices/objects
			if (fieldType == FieldTypeHelper.FieldType.Code || fieldType == FieldTypeHelper.FieldType.Object)
			{
				return "0"; // values for single choice/object fields should be always replaced
			}

			string result;
			switch (settings.OverlayBehavior)
			{
				case Relativity.MassImport.DTO.OverlayBehavior.MergeAll:
					{
						result = "1";
						break;
					}

				case Relativity.MassImport.DTO.OverlayBehavior.ReplaceAll:
					{
						result = "0";
						break;
					}

				case Relativity.MassImport.DTO.OverlayBehavior.UseRelativityDefaults:
					{
						if (overlayMergeValues.HasValue && overlayMergeValues.Value)
						{
							result = "1";
						}
						else
						{
							result = "0";
						}

						break;
					}

				default:
					{
						throw new ArgumentException("Settings.OverlayBehavior cannot be null");
					}
			}

			return result;
		}

		public static bool IsMergeOverlayBehavior(Relativity.MassImport.DTO.OverlayBehavior overlayBehavior, FieldTypeHelper.FieldType fieldType, bool? overlayMergeValues)
		{
			// This function is called only for multi choices/objects
			if (fieldType == FieldTypeHelper.FieldType.Code || fieldType == FieldTypeHelper.FieldType.Object)
			{
				return false; // values for single choice/object fields should be always replaced
			}

			switch (overlayBehavior)
			{
				case Relativity.MassImport.DTO.OverlayBehavior.MergeAll:
					{
						return true;
					}

				case Relativity.MassImport.DTO.OverlayBehavior.ReplaceAll:
					{
						return false;
					}

				case Relativity.MassImport.DTO.OverlayBehavior.UseRelativityDefaults:
					{
						return overlayMergeValues.HasValue && overlayMergeValues.Value;
					}

				default:
					{
						throw new ArgumentException("Value not recognized.", nameof(overlayBehavior));
					}
			}
		}

		public static ErrorFileKey GenerateNonImageErrorFiles(kCura.Data.RowDataGateway.BaseContext context, string runID, int caseArtifactID, int artifactTypeID, bool writeHeader, int keyFieldID)
		{
			var retval = new ErrorFileKey();
			string errorFileName = "";
			string defaultLocation = new kCura.Data.RowDataGateway.Context().ExecuteSqlStatementAsScalar("SELECT (SELECT TOP 1 [Url] FROM [ResourceServer] WHERE [ArtifactID] = [DefaultFileLocationCodeArtifactID]) FROM [Case] WHERE [ArtifactID] = " + caseArtifactID).ToString();
			System.Data.SqlClient.SqlDataReader reader = null;
			try
			{
				reader = context.ExecuteSQLStatementAsReader(ErrorSql(context, runID, keyFieldID));
				if (reader.HasRows)
				{
					errorFileName = Guid.NewGuid().ToString();
					using (var errorFile = new System.IO.StreamWriter(System.IO.Path.Combine(defaultLocation, errorFileName)))
					{
						while (reader.Read())
							errorFile.WriteLine(string.Format("\"{1}{0}{2}{0}{3}\"", "\",\"", reader.GetInt32(0), Relativity.MassImport.DTO.ImportStatusHelper.GetCsvErrorLine(reader.GetInt64(1), reader.GetString(2), "", -1, reader.GetString(2), reader.IsDBNull(3) ? null : reader.GetString(3), reader.IsDBNull(4) ? null : reader.GetString(4)), reader.GetString(2)));
					}
				}
			}
			finally
			{
				kCura.Data.RowDataGateway.Helper.CloseDataReader(reader);
				context.ReleaseConnection();
			}

			retval.LogKey = errorFileName;
			TruncateTempTables(context, runID);
			return retval;
		}

		public static string ErrorSql(kCura.Data.RowDataGateway.BaseContext context, string runID, int keyFieldID)
		{
			string tableName = Constants.NATIVE_TEMP_TABLE_PREFIX + runID;
			string identifierColumnName = context.ExecuteSqlStatementAsScalar(string.Format("SELECT TOP 1 [ColumnName] FROM [ArtifactViewField] INNER JOIN [Field] ON [Field].[ArtifactViewFieldID] = [ArtifactViewField].[ArtifactViewFieldID] AND [Field].[ArtifactID] = {0}", keyFieldID)).ToString();
			string query = $@"
SELECT
	kCura_Import_OriginalLineNumber,
	kCura_Import_Status,
	[{ identifierColumnName }],
	[kCura_Import_DataGridException],
	[kCura_Import_ErrorData]
FROM [Resource].[{ tableName }]
WHERE
	NOT [kCura_Import_Status] = { (long)Relativity.MassImport.DTO.ImportStatus.Pending }
ORDER BY
	kCura_Import_OriginalLineNumber";

			return query;
		}

		public static void TruncateTempTables(kCura.Data.RowDataGateway.BaseContext context, string runID)
		{
			var statements = from tableName in TableNames.GetAllTempTableNames(runID)
				select
					$@" IF EXISTS (SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{ tableName }')
BEGIN
	TRUNCATE TABLE [Resource].[{ tableName }]
END";

			string combinedStatement = string.Join(Environment.NewLine, statements);
			context.ExecuteNonQuerySQLStatement(combinedStatement, Relativity.Data.Config.MassImportSqlTimeout);
		}
	}
}