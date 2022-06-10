using Relativity.Data.MassImport;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.Data
{
	internal class AssociatedObjectsValidationSql
	{
		public static ISqlQueryPart ValidateAssociatedObjectsForSingleObjectField(
			TableNames tableNames, 
			FieldInfo field, 
			int associatedObjectArtifactTypeId)
		{
			return new SerialSqlQuery(CreateErrorsForDuplicatedObjects(tableNames, field), CreateErrorsForMissingChildObjects(tableNames, field, associatedObjectArtifactTypeId));
		}

		public static ISqlQueryPart ValidateAssociatedDocumentForSingleObjectFieldExists(TableNames tableNames, FieldInfo field)
		{
			return new InlineSqlQuery($@"/*Prevent creating associated objects of type document*/
UPDATE N
SET
	[kCura_Import_Status] = [kCura_Import_Status] + {(long)Relativity.MassImport.DTO.ImportStatus.ErrorAssociatedObjectIsDocument}
FROM [Resource].[{tableNames.Native}] N
WHERE
	N.[{field.GetColumnName()}] IS NOT NULL
	AND NOT EXISTS
	(
		SELECT 1
		FROM [Resource].[{tableNames.Native}] N2
		JOIN [Resource].[{tableNames.Part}] P
		ON P.[kCura_Import_ID] = N2.[kCura_Import_ID]
		WHERE
			P.[FieldArtifactID] = {field.ArtifactID}
			AND N2.[{field.GetColumnName()}] = N.[{field.GetColumnName()}]
	)
	AND [kCura_Import_Status] & {(long)Relativity.MassImport.DTO.ImportStatus.ErrorAssociatedObjectIsDocument} = 0;");
		}

		public static string ValidateAssociatedObjectsReferencedByArtifactIdExist(
			TableNames tableNames, 
			FieldInfo field, 
			string associatedObjectTable, 
			string associatedObjectIdentifierColumn)
		{
			return $@"/*create errors for associated objects that do not exist*/
UPDATE N
SET
	[kCura_Import_Status] = [kCura_Import_Status] + {(long)Relativity.MassImport.DTO.ImportStatus.ErrorAssociatedObjectIsMissing},
	[kCura_Import_ErrorData] = '{field.DisplayName}|' + N.[{field.GetColumnName()}] + '|{associatedObjectTable}'
FROM [Resource].[{tableNames.Native}] N
WHERE
	N.[{field.GetColumnName()}] IS NOT NULL
	AND NOT EXISTS
	(
		SELECT 1
		FROM [{associatedObjectTable}] 
		WHERE [{associatedObjectTable}].[{associatedObjectIdentifierColumn}] = N.[{field.GetColumnName()}]
	)
	AND [kCura_Import_Status] & {(long)Relativity.MassImport.DTO.ImportStatus.ErrorAssociatedObjectIsMissing} = 0;";
		}

		private static InlineSqlQuery CreateErrorsForDuplicatedObjects(
			TableNames tableNames, 
			FieldInfo field)
		{
			return new InlineSqlQuery($@"
;WITH DuplicatedAssociatedObjects(kCura_Import_ID) AS
(
	SELECT P.[kCura_Import_ID]
	FROM [Resource].[{tableNames.Part}] P
	WHERE P.[FieldArtifactID] = {field.ArtifactID}
	GROUP BY P.[kCura_Import_ID]
	HAVING COUNT(P.[kCura_Import_ID]) > 1
)

UPDATE N2
SET
	N2.[kCura_Import_Status] = N2.[kCura_Import_Status] + {(long)Relativity.MassImport.DTO.ImportStatus.ErrorDuplicateAssociatedObject}
FROM
	[Resource].[{tableNames.Native}] N
JOIN [DuplicatedAssociatedObjects] ON
	N.[kCura_Import_ID] = [DuplicatedAssociatedObjects].[kCura_Import_ID]
JOIN [Resource].[{tableNames.Native}] N2 ON
	N.[{field.GetColumnName()}] = N2.[{field.GetColumnName()}]
WHERE N2.[kCura_Import_Status] & {(long)Relativity.MassImport.DTO.ImportStatus.ErrorDuplicateAssociatedObject} = 0;");
		}

		private static InlineSqlQuery CreateErrorsForMissingChildObjects(
			TableNames tableNames, 
			FieldInfo field, 
			int associatedObjectArtifactTypeId)
		{
			return new InlineSqlQuery($@"
DECLARE @objectTypeIsChild INT;
SET @objectTypeIsChild =
CASE
	WHEN EXISTS
		(SELECT * FROM [ObjectType]
		WHERE
			[ObjectType].[DescriptorArtifactTypeID] = {associatedObjectArtifactTypeId}
			AND NOT [ObjectType].ParentArtifactTypeID = {(int) Relativity.ArtifactType.Case}
			AND NOT [ObjectType].ParentArtifactTypeID = {(int) Relativity.ArtifactType.Folder}
		)
	THEN 1
	ELSE 0
END;

UPDATE N
SET
	[kCura_Import_Status] = [kCura_Import_Status] + {(long)Relativity.MassImport.DTO.ImportStatus.ErrorAssociatedObjectIsChild},
    [kCura_Import_ErrorData] = @fieldDisplayName
FROM [Resource].[{tableNames.Native}] N
WHERE
	@objectTypeIsChild = 1
	AND N.[{field.GetColumnName()}] IS NOT NULL
	AND NOT EXISTS
	(
		SELECT 1
		FROM [Resource].[{tableNames.Native}] N2
		JOIN [Resource].[{tableNames.Part}] P
		ON P.[kCura_Import_ID] = N2.[kCura_Import_ID]
		WHERE
			P.[FieldArtifactID] = {field.ArtifactID}
			AND N2.[{field.GetColumnName()}] = N.[{field.GetColumnName()}]
	)	
	AND N.[kCura_Import_Status] & {(long)Relativity.MassImport.DTO.ImportStatus.ErrorAssociatedObjectIsChild} = 0;");
		}
	}
}