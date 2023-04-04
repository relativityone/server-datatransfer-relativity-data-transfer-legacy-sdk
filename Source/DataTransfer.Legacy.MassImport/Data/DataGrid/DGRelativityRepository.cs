using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using kCura.Data.RowDataGateway;
using Relativity.DataGrid.DTOs;
using Relativity.DataGrid.Interfaces.DGFS;

namespace Relativity.MassImport.Data.DataGrid
{
	internal class DGRelativityRepository : IRelativityRepository
	{
		private const string _DG_TEMP_TABLE_NAME = "#DgImportFileInfos";

		public static string UpdateDgFieldMappingRecordsSql(string tableName, string statusColumnName, bool hasLinkedTextColumn)
		{
			if (hasLinkedTextColumn)
			{
				return $@"
WITH dgImportFileInfoFull AS
(
	SELECT P.[ArtifactID], DG.[FieldArtifactId], DG.[FileLocation], DG.[FileSize], DG.[Checksum], DG.[ImportId], DG.[LinkedText]
	FROM {_DG_TEMP_TABLE_NAME} AS DG
	JOIN [Resource].[{tableName}] AS P ON P.[kCura_Import_ID] = DG.[ImportId]
	WHERE P.[{statusColumnName}] = {(long)ImportStatus.Pending}
)
MERGE INTO DataGridFileMapping AS T
USING dgImportFileInfoFull AS S
ON T.FieldArtifactID = S.FieldArtifactId AND T.ArtifactID = S.ArtifactID
WHEN MATCHED AND S.FileLocation IS NULL AND S.FileSize = 0 THEN
	DELETE
WHEN MATCHED THEN
	UPDATE SET 
		T.FileLocation = S.FileLocation,
		T.FileSize = S.FileSize,
		T.UpdatedDate = GETUTCDATE(),
		T.Checksum = S.Checksum,
		T.LinkedText = S.LinkedText
WHEN NOT MATCHED AND NOT (S.FileLocation IS NULL AND S.FileSize = 0) THEN
	INSERT (ArtifactId, FieldArtifactId, FileLocation, FileSize, CreatedDate, Checksum, LinkedText)
	VALUES (S.ArtifactId, S.FieldArtifactId, S.FileLocation, S.FileSize, GETUTCDATE(), S.Checksum, S.LinkedText)
OUTPUT S.[ImportId], $ACTION;

DROP TABLE {_DG_TEMP_TABLE_NAME}
";
			}
			else
			{
				return $@"
WITH dgImportFileInfoFull AS
(
	SELECT P.[ArtifactID], DG.[FieldArtifactId], DG.[FileLocation], DG.[FileSize], DG.[Checksum], DG.[ImportId]
	FROM {_DG_TEMP_TABLE_NAME} AS DG
	JOIN [Resource].[{tableName}] AS P ON P.[kCura_Import_ID] = DG.[ImportId]
	WHERE P.[{statusColumnName}] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
)
MERGE INTO DataGridFileMapping AS T
USING dgImportFileInfoFull AS S
ON T.FieldArtifactID = S.FieldArtifactId AND T.ArtifactID = S.ArtifactID
WHEN MATCHED AND S.FileLocation IS NULL AND S.FileSize = 0 THEN
	DELETE
WHEN MATCHED THEN
	UPDATE SET 
		T.FileLocation = S.FileLocation,
		T.FileSize = S.FileSize,
		T.UpdatedDate = GETUTCDATE(),
		T.Checksum = S.Checksum
WHEN NOT MATCHED AND NOT (S.FileLocation IS NULL AND S.FileSize = 0) THEN
	INSERT (ArtifactId, FieldArtifactId, FileLocation, FileSize, CreatedDate, Checksum)
	VALUES (S.ArtifactId, S.FieldArtifactId, S.FileLocation, S.FileSize, GETUTCDATE(), S.Checksum)
OUTPUT S.[ImportId], $ACTION;

IF OBJECT_ID('tempdb..{_DG_TEMP_TABLE_NAME}') IS NOT NULL DROP TABLE {_DG_TEMP_TABLE_NAME}
";
			}
		}

		public static string CreateDgFieldMappingTempTableSql()
		{
			return $@"
IF OBJECT_ID('tempdb..{_DG_TEMP_TABLE_NAME}') IS NOT NULL DROP TABLE {_DG_TEMP_TABLE_NAME}

CREATE TABLE {_DG_TEMP_TABLE_NAME} (
	[{nameof(DGImportFileInfo.ImportId)}] INT NULL,
	[{nameof(DGImportFileInfo.FieldArtifactId)}] INT NULL,
	[{nameof(DGImportFileInfo.FileLocation)}] NVARCHAR(2000) NULL,
	[{nameof(DGImportFileInfo.FileSize)}] BIGINT NULL,
	[{nameof(DGImportFileInfo.Checksum)}] NVARCHAR(MAX) NULL,
	[{nameof(DGImportFileInfo.LinkedText)}] BIT NULL
)

SELECT COUNT(*) FROM sys.columns WHERE Name = N'{nameof(DGImportFileInfo.LinkedText)}' AND Object_ID = Object_ID(N'EDDSDBO.DataGridFileMapping')
";
		}

		public static SqlBulkCopyParameters GetDgFieldMappingTempTableBulkCopyParameters()
		{
			var bulkCopyParameters = new SqlBulkCopyParameters()
			{
				DestinationTableName = _DG_TEMP_TABLE_NAME
			};

			return bulkCopyParameters;
		}

		public ConcurrentBag<DGImportFileInfo> ImportFileInfos = new ConcurrentBag<DGImportFileInfo>();

		public Task RemoveFileInformation(RecordIdentity record, FieldIdentity field)
		{
			// They were never added to the [DataGridFileMapping] so we don't have to delete it.
			return Task.CompletedTask;
		}

		public Task RecordFileInformation(RecordIdentity record, FieldIdentity field, string path, ulong byteSize, string checksum)
		{
			return RecordFileInformation(record, field, path, byteSize, checksum, false);
		}

		public Task RecordFileInformation(RecordIdentity record, FieldIdentity field, string path, ulong byteSize, string checksum, bool isLinkedText)
		{
			var importFileInfo = new DGImportFileInfo()
			{
				ImportId = record.ArtifactID,
				FieldArtifactId = field.ArtifactID,
				FieldName = field.FieldName,
				FieldNamespace = field.FieldNamespace,
				FileLocation = path,
				FileSize = byteSize,
				Checksum = checksum,
				IndexName = record.IndexName,
				LinkedText = isLinkedText,
			};
			ImportFileInfos.Add(importFileInfo);

			return Task.CompletedTask;
		}
	}
}