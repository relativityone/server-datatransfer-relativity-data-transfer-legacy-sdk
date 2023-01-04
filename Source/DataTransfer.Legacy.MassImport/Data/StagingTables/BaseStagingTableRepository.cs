using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.Data.RowDataGateway;
using kCura.Utility;
using Relativity.Data.MassImport;
using Relativity.MassImport.Data.Cache;

namespace Relativity.MassImport.Data.StagingTables
{
	using Relativity.Logging;
	using Relativity.MassImport.DTO;

	internal abstract class BaseStagingTableRepository : IStagingTableRepository
	{
		private int? _timeoutValue;
		protected readonly TableNames TableNames;
		protected readonly BaseContext Context;

		public BaseStagingTableRepository(BaseContext context, TableNames tableNames, ImportMeasurements importMeasurements)
		{
			Context = context;
			TableNames = tableNames;
			ImportMeasurements = importMeasurements;
		}

		public bool StagingTablesExist()
		{
			string sql = $"IF EXISTS(SELECT 1 FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{TableNames.Native}') SELECT 1 ELSE SELECT 0;";
			bool exists = Context.ExecuteSqlStatementAsScalar<bool>(sql);
			return exists;
		}

		public abstract void CreateStagingTables(
			ColumnDefinitionCache columnDefinitionCache,
			Relativity.MassImport.DTO.NativeLoadInfo settings, bool includeExtractedTextEncoding,
			bool excludeFolderPathForOldClient);

		public void TruncateStagingTables(
			FieldInfo[] mappedFields,
			bool LoadImportedFullTextFromServer)
		{
			string sql = $@"
TRUNCATE TABLE [Resource].[{TableNames.Native}]
TRUNCATE TABLE [Resource].[{TableNames.Code}]
TRUNCATE TABLE [Resource].[{TableNames.Part}]
TRUNCATE TABLE [Resource].[{TableNames.Parent}]
TRUNCATE TABLE [Resource].[{TableNames.Objects}]
TRUNCATE TABLE [Resource].[{TableNames.Map}]";
			if (LoadImportedFullTextFromServer)
			{
				var fullTextField = mappedFields?.FirstOrDefault(x => x.Category == FieldCategory.FullText);
				if (this.LoadImportedFullTextFromServer(fullTextField))
				{
					sql = sql + $"TRUNCATE TABLE [Resource].[{TableNames.FullText}]";
				}
			}

			Context.ExecuteNonQuerySQLStatement(sql, QueryTimeout);
		}

		public string BulkInsert(Relativity.MassImport.DTO.NativeLoadInfo settings, string bulkFileSharePath, ILog logger)
		{
			try
			{
				ImportMeasurements.SqlBulkImportTime.Start();
				string sqlText = this.GetBulkInsertQuery(settings, bulkFileSharePath, settings.DataFileName, TableNames.Native);
				ExecuteBulkLoadWithRetryOnSqlTemporaryError(sqlText, logger, ImportMeasurements);

				sqlText = this.GetBulkInsertQuery(settings, bulkFileSharePath, settings.CodeFileName, TableNames.Code);
				ExecuteBulkLoadWithRetryOnSqlTemporaryError(sqlText, logger, ImportMeasurements);

				sqlText = this.GetBulkInsertQuery(settings, bulkFileSharePath, settings.ObjectFileName, TableNames.Objects);
				ExecuteBulkLoadWithRetryOnSqlTemporaryError(sqlText, logger, ImportMeasurements);
			}
			catch (ExecuteSQLStatementFailedException ex)
			{
				if (BulkLoadSqlErrorRetryHelper.IsTooMuchDataForSqlError(ex))
				{
					throw new Exception("Data exceeds the maximum size SQL Server allows.", ex);
				}
				else
				{
					throw;
				}
			}
			finally
			{
				kCura.Utility.File.Instance.Delete(Path.Combine(bulkFileSharePath, settings.DataFileName));
				kCura.Utility.File.Instance.Delete(Path.Combine(bulkFileSharePath, settings.CodeFileName));
				kCura.Utility.File.Instance.Delete(Path.Combine(bulkFileSharePath, settings.ObjectFileName));
				ImportMeasurements.SqlBulkImportTime.Stop();
			}

			InjectionManager.Instance.Evaluate("1a9e1342-9f1e-4255-9045-d4e18d53b511");
			return TableNames.RunId;
		}

		public abstract string Insert(ColumnDefinitionCache columnDefinitionCache, Relativity.MassImport.DTO.NativeLoadInfo settings, bool excludeFolderPathForOldClient);

		public IDictionary<int, int> ReadNumberOfChoicesPerCodeTypeId()
		{
			const int timeoutInSeconds = 3;
			const string codeTypeIdColumn = "CodeTypeId";
			const string numberOfChoicesColumn = "NumberOfChoices";

			string sql = $@"SELECT {codeTypeIdColumn}, COUNT(*) As {numberOfChoicesColumn} FROM [Resource].[{TableNames.Code}] GROUP BY {codeTypeIdColumn};";

			var result = new Dictionary<int, int>();
			using (var reader = Context.ExecuteSQLStatementAsReader(sql, timeoutInSeconds))
			{
				while (reader.Read())
				{
					int codeTypeId = (int)reader[codeTypeIdColumn];
					int numberOfChoices = (int)reader[numberOfChoicesColumn];

					result[codeTypeId] = numberOfChoices;
				}
			}
			return result;
		}

		protected ImportMeasurements ImportMeasurements { get; set; }

		protected int QueryTimeout
		{
			get
			{
				if (!_timeoutValue.HasValue)
				{
					_timeoutValue = Relativity.Data.Config.MassImportSqlTimeout;
				}

				return _timeoutValue.Value;
			}
		}

		protected void CreateStagingTablesBase(ColumnDefinitionCache columnDefinitionCache, FieldInfo[] mappedFields, string metadataColumnClause, int keyFieldID, bool LoadImportedFullTextFromServer)
		{
			string sql = @"
/*
Format Replace:
	-----------
	0: native temp table name
	1: metadata columns
	2: codeartifact temp table name
	3: maximum length for document identifier
	4: part table name
	5: document identifier column collation
	6: objects temp table name
	7: mapping table name
	8: parent table name
*/
IF NOT EXISTS(SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{0}')
BEGIN
	CREATE TABLE [Resource].[{0}] (
		[kCura_Import_ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
		[kCura_Import_Status] BIGINT NOT NULL,
		[kCura_Import_IsNew] BIT NOT NULL,
		[ArtifactID] INT NOT NULL,
		[kCura_Import_OriginalLineNumber] INT NOT NULL,
		[kCura_Import_FileGuid] NVARCHAR(100) NOT NULL,
		[kCura_Import_Filename] NVARCHAR(200) NOT NULL,
		[kCura_Import_Location] NVARCHAR(2000),
		[kCura_Import_OriginalFileLocation] NVARCHAR(2000),
		[kCura_Import_FileSize] BIGINT NOT NULL,
		[kCura_Import_ParentFolderID] INT NOT NULL,

{1}
		[kCura_Import_DataGridException] NVARCHAR(MAX),
		[kCura_Import_ErrorData] NVARCHAR(MAX) NULL
	)
END

IF NOT EXISTS(SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{2}')
BEGIN
	CREATE TABLE [Resource].[{2}] (
		[DocumentIdentifier] NVARCHAR({3}) COLLATE {5} NOT NULL,
		[CodeArtifactID] INT NOT NULL,
		[CodeTypeID] INT NOT NULL,

		INDEX IX_DocumentIdentifier CLUSTERED 
		(
			[DocumentIdentifier] ASC
		)
	)
END

IF NOT EXISTS(SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{6}')
BEGIN
	CREATE TABLE [Resource].[{6}] (
		[DocumentIdentifier] NVARCHAR({3}) COLLATE {5} NOT NULL,
		[ObjectName] NVARCHAR(450) COLLATE {5} NOT NULL,
		[ObjectArtifactID] INT NOT NULL,
		[ObjectTypeID] INT NOT NULL,
		[FieldID] INT NOT NULL,

		INDEX IX_DocumentIdentifier CLUSTERED 
		(
			[DocumentIdentifier] ASC
		)
	)

	CREATE NONCLUSTERED INDEX [IX_FieldID] ON [Resource].[{6}] ([FieldID] ASC)
END

IF NOT EXISTS(SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{4}')
BEGIN
	CREATE TABLE [Resource].[{4}] (
		[kCura_Import_ID] INT NOT NULL,
		[kCura_Import_IsNew] BIT NOT NULL,
		[ArtifactID] INT NOT NULL,
		[AccessControlListID] INT NULL,
		[FieldArtifactID] INT NOT NULL,

		INDEX IX_kCuraImportID_FieldArtifactId_kCuraImportIsNew CLUSTERED 
		(
			[kCura_Import_ID] ASC,
			[FieldArtifactID] ASC,
			[kCura_Import_IsNew] ASC
		)
	)
	CREATE NONCLUSTERED INDEX IX_FieldArtifactID
	ON [Resource].[{4}] ([FieldArtifactID] ASC)
	INCLUDE ([ArtifactID])
	WHERE [kCura_Import_IsNew] = 1
END

IF NOT EXISTS(SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{8}')
BEGIN
	CREATE TABLE [Resource].[{8}] (
		[kCura_Import_ID] INT NOT NULL,
		[ParentArtifactID] INT NOT NULL,
		[ParentArtifactTypeID] INT NOT NULL,
		[ParentAccessControlListID] INT NOT NULL,

		INDEX IX_kCuraImportID CLUSTERED 
		(
			[kCura_Import_ID] ASC
		)
	)
END

IF NOT EXISTS(SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{7}')
BEGIN
	CREATE TABLE [Resource].[{7}] (
		[ArtifactID] INT NOT NULL,
		[MappedArtifactID] INT NOT NULL,
		[FieldArtifactID] INT NOT NULL,
		[IsNew] BIT NOT NULL,
		CONSTRAINT PK_{7} PRIMARY KEY (ArtifactID, FieldArtifactID, IsNew, MappedArtifactID)
	)
END
";
			if (LoadImportedFullTextFromServer)
			{
				var fullTextField = mappedFields?.FirstOrDefault(x => x.Category == FieldCategory.FullText);
				if (this.LoadImportedFullTextFromServer(fullTextField))
				{
					sql = sql + $@"
IF NOT EXISTS(SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{ TableNames.FullText }')
BEGIN
	CREATE TABLE [Resource].[{ TableNames.FullText }](
		[kCura_Import_ID] INT NOT NULL PRIMARY KEY,
		[{ FullTextFieldColumnName(fullTextField) }] NVARCHAR(MAX)
	)
END
";
				}
			}

			var idField = mappedFields.Single(p => p.ArtifactID == keyFieldID);
			sql = string.Format(
				sql,
				TableNames.Native,
				metadataColumnClause,
				TableNames.Code,
				idField.TextLength,
				TableNames.Part,
				columnDefinitionCache[keyFieldID].CollationName,
				TableNames.Objects,
				TableNames.Map,
				TableNames.Parent
			);

			Context.ExecuteNonQuerySQLStatement(sql, QueryTimeout);
		}

		protected string GetColumnDefinition(ColumnDefinitionCache columnDefinitionCache, FieldInfo mappedField)
		{
			var info = columnDefinitionCache[mappedField.ArtifactID];
			return info.GetColumnDescription(mappedField);
		}

		private void ExecuteBulkLoadWithRetryOnSqlTemporaryError(string sqlText, ILog logger, ImportMeasurements importMeasurements)
		{
			BulkLoadSqlErrorRetryHelper.RetryOnBulkLoadSqlTemporaryError(() => Context.ExecuteNonQuerySQLStatement(sqlText, QueryTimeout), logger, importMeasurements);
		}

		private string GetBulkInsertQuery(Relativity.MassImport.DTO.NativeLoadInfo settings, string bulkFileSharePath, string bulkFileName, string tableName)
		{
			if (string.IsNullOrWhiteSpace(bulkFileSharePath))
			{
				throw new ArgumentException("The BCP share path cannot be null or empty.", nameof(bulkFileSharePath));
			}

			if (string.IsNullOrWhiteSpace(bulkFileName))
			{
				throw new ArgumentException("The BCP file name cannot be null or empty.", nameof(bulkFileName));
			}

			if (string.IsNullOrWhiteSpace(tableName))
			{
				throw new ArgumentException("The BCP table name cannot be null or empty.", nameof(tableName));
			}

			// REL-165495: SQL bulk insert should include TABLOCK and batch setting. The constant 10MB matches the default batch size setting.
			const int DefaultKBPerBatch = 10000;
			string friendlyTableName = SqlNameHelper.GetSqlFriendlyName(tableName);
			string baseImportString = @"BULK INSERT [Resource].[{0}] FROM '{1}' WITH (FIELDTERMINATOR='{2}',ROWTERMINATOR='{2}\n',DATAFILETYPE='widechar', TABLOCK, KILOBYTES_PER_BATCH={3});";
			string filePath = Path.Combine(bulkFileSharePath, bulkFileName);
			return string.Format(baseImportString, friendlyTableName, filePath, settings.BulkLoadFileFieldDelimiter, DefaultKBPerBatch);
		}

		private bool LoadImportedFullTextFromServer(FieldInfo fullTextField)
		{
			return fullTextField != null && !fullTextField.EnableDataGrid;
		}

		private string FullTextFieldColumnName(FieldInfo fullTextField)
		{
			return fullTextField?.GetColumnName();
		}
	}
}