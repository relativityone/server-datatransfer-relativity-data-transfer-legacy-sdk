using System.Collections.Concurrent;
using System.Threading.Tasks;
using Relativity.DataGrid.DTOs;
using Relativity.DataGrid.Interfaces.DGFS;

namespace Relativity.MassImport.Data.DataGrid
{
	internal class DGRelativityRepository : IRelativityRepository
	{
		public static string UpdateDgFieldMappingRecordsSql(string tableName, string statusColumnName)
		{
			return $@"
WITH dgImportFileInfoFull AS
(
	SELECT P.[ArtifactID], DG.[FieldArtifactId], DG.[FileLocation], DG.[FileSize], DG.[Checksum], DG.[ImportId]
	FROM @dgImportFileInfo AS DG
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
";
		}

		public ConcurrentBag<DGImportFileInfo> ImportFileInfos = new ConcurrentBag<DGImportFileInfo>();

		public Task RecordFileInformation(RecordIdentity record, FieldIdentity field, string path, ulong byteSize, string checksum)
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
			};
			ImportFileInfos.Add(importFileInfo);

			return Task.CompletedTask;
		}

		public Task RemoveFileInformation(RecordIdentity record, FieldIdentity field)
		{
			// They were never added to the [DataGridFileMapping] so we don't have to delete it.
			return Task.CompletedTask;
		}
	}
}