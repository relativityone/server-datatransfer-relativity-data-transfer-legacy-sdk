using System;
using Relativity.Data.MassImport;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.Data
{
	internal class NativeImportSql
	{
		public virtual InlineSqlQuery PopulatePartTable(TableNames tableNames, string documentTable, int topFieldArtifactID, string keyField)
		{
			return new InlineSqlQuery($@"
INSERT INTO [Resource].[{tableNames.Part}]
SELECT
	N.[kCura_Import_ID],
	0 [kCura_Import_IsNew],
	D.[ArtifactID],
	D.[AccessControlListID_D],
	{topFieldArtifactID} [FieldArtifactID]
FROM [Resource].[{tableNames.Native}] N
JOIN [{documentTable}] D ON D.[{keyField}] = N.[{keyField}];
");
		}

		public InlineSqlQuery PopulateParentTable(TableNames tableNames)
		{
			return new InlineSqlQuery($@"
INSERT INTO [Resource].[{tableNames.Parent}]
SELECT
	N.[kCura_Import_ID],
	A.ArtifactID [ParentArtifactID],
	A.ArtifactTypeID [ParentArtifactTypeID],
	A.AccessControlListID [ParentAccessControlListID]
FROM [Resource].[{tableNames.Native}] N
JOIN [Artifact] A ON A.[ArtifactID] = N.[kCura_Import_ParentFolderID]
LEFT JOIN [Resource].[{tableNames.Part}] P ON N.[kCura_Import_ID] = P.[kCura_Import_ID]
WHERE P.[ArtifactID] IS NULL;
");
		}

		public InlineSqlQuery PopulateAssociatedPartTable(TableNames tableNames, int fieldArtifactId, string associatedObjectTable, string textIdentifierColumn, string keyFieldColumn)
		{
			return new InlineSqlQuery($@"
INSERT INTO [Resource].[{tableNames.Part}]
SELECT
	N.[kCura_Import_ID],
	0 [kCura_Import_IsNew],
	D.[ArtifactID],
	NULL,
	{fieldArtifactId} [FieldArtifactID]
FROM (
	SELECT
		MIN(N.[kCura_Import_ID]) [kCura_Import_ID],
		N.[{textIdentifierColumn}]
	FROM [Resource].[{tableNames.Native}] N
	GROUP BY N.[{textIdentifierColumn}]
) N
JOIN [{associatedObjectTable}] D ON D.[{keyFieldColumn}] = N.[{textIdentifierColumn}];
");
		}

		public InlineSqlQuery AppendOnlyErrors(TableNames tableNames)
		{
			return new InlineSqlQuery($@"
UPDATE N
SET
	[kCura_Import_Status] = [kCura_Import_Status] + {(long)Relativity.MassImport.DTO.ImportStatus.ErrorAppend}
FROM [Resource].[{tableNames.Native}] N
WHERE EXISTS
(
	SELECT 1 FROM [Resource].[{tableNames.Part}] P WHERE P.[kCura_Import_ID] = N.[kCura_Import_ID]
);
");
		}

		public InlineSqlQuery AppendParentMissingErrors(TableNames tableNames)
		{
			return new InlineSqlQuery($@"
UPDATE N
SET
	[kCura_Import_Status] = [kCura_Import_Status] + {(long)Relativity.MassImport.DTO.ImportStatus.ErrorAppendNoParent}
FROM [Resource].[{tableNames.Native}] N
WHERE N.[kCura_Import_ParentFolderID] = -1 AND N.[kCura_Import_Status] = 0;
");
		}

		public string IncomingDocumentCount(string parentTable)
		{
			return $"SELECT count(*) FROM [Resource].{parentTable}";
		}

		public InlineSqlQuery AppendOverlayParentMissingErrors(TableNames tableNames)
		{
			return new InlineSqlQuery($@"
UPDATE N
SET
	[kCura_Import_Status] = [kCura_Import_Status] + {(long)Relativity.MassImport.DTO.ImportStatus.ErrorAppendNoParent}
FROM [Resource].[{tableNames.Native}] N
WHERE N.[kCura_Import_ParentFolderID] = -1
	AND NOT EXISTS
	(
		SELECT 1 FROM [Resource].[{tableNames.Part}] P WHERE P.[kCura_Import_ID] = N.[kCura_Import_ID]
	);
");
		}

		public InlineSqlQuery ConvertObjectFieldsToArtifactIDs(TableNames tableNames, FieldInfo field)
		{
			// This query updates a single object field in N table. It converts single-object column from identifier strings into artifactIds. 
			// The single-object column has non unique values but there will be only one row in the P table per value. We join first time between N and P table
			// to get the ArtifactId for the value and then we join again with the N table (aliased as N2) to extend the selection to all the rows with
			// that value.
			return new InlineSqlQuery($@"
UPDATE
	N2
SET
	N2.[{field.GetColumnName()}] = CAST(P.[ArtifactID] AS NVARCHAR(max))
FROM
	[Resource].[{tableNames.Native}] N
JOIN [Resource].[{tableNames.Part}] P ON 
	P.[kCura_Import_ID] = N.[kCura_Import_ID] AND P.[FieldArtifactID] = {field.ArtifactID}
JOIN [Resource].[{tableNames.Native}] N2 ON
	N2.[{field.GetColumnName()}] = N.[{field.GetColumnName()}]
WHERE
	N2.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending};
");
		}

		public InlineSqlQuery InsertAncestorsOfTopLevelObjectsLegacy(TableNames tableNames)
		{
			return new InlineSqlQuery($@"
INSERT INTO ArtifactAncestry(
	ArtifactID,
	AncestorArtifactID
)	SELECT
		P.ArtifactID, AncestorArtifactID
	FROM
		[Resource].[{tableNames.Part}] P
	JOIN [Resource].[{tableNames.Parent}] PARENT ON P.[kCura_Import_ID] = PARENT.[kCura_Import_ID]
	INNER JOIN ArtifactAncestry ON ArtifactAncestry.ArtifactID = PARENT.ParentArtifactID
	WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = 0
	UNION ALL
	SELECT
		P.ArtifactID, AncestorArtifactID = PARENT.ParentArtifactID
	FROM
		[Resource].[{tableNames.Part}] P
	JOIN [Resource].[{tableNames.Parent}] PARENT ON P.[kCura_Import_ID] = PARENT.[kCura_Import_ID]
	WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = 0;
");
		}

		public SerialSqlQuery InsertAncestorsOfTopLevelObjects(TableNames tableNames)
		{
			var sql = new SerialSqlQuery();

			sql.Add(new InlineSqlQuery($@"
DROP TABLE IF EXISTS [{tableNames.ParentAncestors}], [{tableNames.NewAncestors}];
"));

			// add parent as the new record ancestor
			sql.Add(new InlineSqlQuery($@"
SELECT P.ArtifactID, PARENT.ParentArtifactID
INTO [{tableNames.NewAncestors}]
FROM [Resource].[{tableNames.Part}] P
JOIN [Resource].[{tableNames.Parent}] PARENT ON P.[kCura_Import_ID] = PARENT.[kCura_Import_ID]
WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = 0;
"));

			sql.Add(new InlineSqlQuery($@"
ALTER TABLE [{tableNames.NewAncestors}] ADD PRIMARY KEY(ArtifactID);
"));

			// add all parent ancestors as the new record ancestors
			sql.Add(new InlineSqlQuery($@"
SELECT N.ArtifactID, A.AncestorArtifactID
INTO [{tableNames.ParentAncestors}]
FROM [{tableNames.NewAncestors}] N
JOIN [EDDSDBO].ArtifactAncestry A ON A.ArtifactID = N.ParentArtifactID;
"));

			sql.Add(new InlineSqlQuery($@"
ALTER TABLE [{tableNames.ParentAncestors}] ADD PRIMARY KEY(ArtifactID,AncestorArtifactID);
"));

			// insert all ancestors for the new record
			sql.Add(new InlineSqlQuery($@"
INSERT INTO [EDDSDBO].ArtifactAncestry(ArtifactID, AncestorArtifactID)
SELECT ArtifactID, ParentArtifactID FROM [{tableNames.NewAncestors}]
UNION ALL
SELECT ArtifactID, AncestorArtifactID FROM [{tableNames.ParentAncestors}];
"));

			return sql;
		}

		public InlineSqlQuery InsertAncestorsOfAssociateObjects(TableNames tableNames, string fieldArtifactId, string topLevelParentArtifactId)
		{
			return new InlineSqlQuery($@"
INSERT INTO ArtifactAncestry(
	ArtifactID,
	AncestorArtifactID
)	SELECT
		P.ArtifactID, {topLevelParentArtifactId} AncestorArtifactID
	FROM
		[Resource].[{tableNames.Part}] P
	WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = {fieldArtifactId};
");
		}

		public InlineSqlQuery CreateAuditClause(TableNames tableNames, int fieldArtifactId, string requestOrigination, string recordOrigination)
		{
			return new InlineSqlQuery($@"
/*
Parameters:
	@auditUserID
*/
INSERT INTO AuditRecord (
	[ArtifactID],
	[Action],
	[Details],
	[UserID],
	[TimeStamp],
	[RequestOrigination],
	[RecordOrigination]
)	SELECT
		P.[ArtifactID],
		2,
		'',
		@auditUserID,
		GETUTCDATE(),
		'{requestOrigination}',
		'{recordOrigination}'
	FROM
		[Resource].[{tableNames.Part}] P
	WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = {fieldArtifactId};
");
		}

		public string CreateAuditClauseForMultiObject(string requestOrigination, string recordOrigination)
		{
			return $@"
INSERT INTO AuditRecord (
	[ArtifactID],
	[Action],
	[Details],
	[UserID],
	[TimeStamp],
	[RequestOrigination],
	[RecordOrigination]
)	SELECT
		P.[ArtifactID],
		2,
		'',
		@auditUserID,
		GETUTCDATE(),
		'{requestOrigination}',
		'{recordOrigination}'
	FROM
		#InsertedArtifactIDsTable P;
";
		}

		public string CreateMultipleAssociativeObjects(TableNames tableNames, string associatedObjectsTable, string associatedObjectsId, string keyFieldColumnName)
		{
			return $@"/*
	0: Objects Temp Table
	1: Object Table Name
	2: Object Table Identifier
	@userID
	@artifactType
	@fieldID
	@parentId: Top Level Parent ArtifactId
	@parentAccessControlListId: Top Level Parent AccessControlListID
	*/

	CREATE TABLE #InsertedArtifactIDsTable (
		ArtifactID INT,
		ObjectName NVARCHAR(450) COLLATE DATABASE_DEFAULT,

		INDEX IX_ObjectName CLUSTERED
		(
			ObjectName ASC
		)
	);

	DECLARE @now DATETIME SET @now = GETUTCDATE()
	
	INSERT INTO Artifact (
		[ArtifactTypeID],
		[ParentArtifactID],
		[AccessControlListID],
		[AccessControlListIsInherited],
		[CreatedOn],
		[LastModifiedOn],
		[CreatedBy],
		[LastModifiedBy],
		[TextIdentifier],
		[ContainerID],
		[Keywords],
		[Notes],
		[DeleteFlag]
	)
	OUTPUT INSERTED.ArtifactID, INSERTED.TextIdentifier INTO #InsertedArtifactIDsTable
	SELECT DISTINCT
		[ArtifactTypeID] = @artifactType,
		[ParentArtifactID] = @parentId,
		[AccessControlListID] = @parentAccessControlListId,
		[AccessControlListIsInherited] = 1,
		[CreatedOn] = @now,
		[LastModifiedOn] = @now,
		[CreatedBy] = @userID,
		[LastModifiedBy] = @userID,
		[TextIdentifier] = O.[ObjectName],
		[ContainerID] = @parentId,
		[Keywords] = '',
		[Notes] = '',
		[DeleteFlag] = 0
	FROM [Resource].[{tableNames.Objects}] O
	JOIN [Resource].[{tableNames.Native}] N
		ON O.DocumentIdentifier = N.[{keyFieldColumnName}]
	WHERE
		O.[ObjectArtifactID] = -1
		AND
		O.[FieldID] = @fieldID
		AND
		N.[kCura_Import_Status] = 0;

/* SelfReferencedMultiField */

INSERT INTO [{associatedObjectsTable}] (
	[ArtifactID],
	[{associatedObjectsId}]
)
SELECT ArtifactID, ObjectName
FROM #InsertedArtifactIDsTable

UPDATE O
SET ObjectArtifactID=(SELECT ArtifactID from #InsertedArtifactIDsTable WHERE #InsertedArtifactIDsTable.ObjectName COLLATE DATABASE_DEFAULT = O.ObjectName COLLATE DATABASE_DEFAULT)
FROM [Resource].[{tableNames.Objects}] O
	JOIN [Resource].[{tableNames.Native}] N
		ON O.DocumentIdentifier = N.[{keyFieldColumnName}]
WHERE
	O.[ObjectArtifactID] = -1
	AND
	O.[FieldID] = @fieldID
	AND
	N.[kCura_Import_Status] = 0;

/* ImportAuditClase */

INSERT INTO [ArtifactAncestry](
	ArtifactID,
	AncestorArtifactID
)	SELECT DISTINCT
		ArtifactID, AncestorArtifactID = @parentId
	FROM
		#InsertedArtifactIDsTable ";
		}

		public FormattableString ValidateReferencedObjectsAreNotDuplicated(TableNames tableNames, string keyFieldColumnName, string associatedObjectTable, string associatedObjectIdentifierColumn, string associatedArtifactTypeID)
		{
			return $@"
			;WITH DuplicatedObjects(ObjectName) AS
			(
				SELECT [{associatedObjectIdentifierColumn}] as ObjectName
				FROM [{associatedObjectTable}]
				GROUP BY [{associatedObjectIdentifierColumn}]
				HAVING COUNT(*) > 1
			)
			UPDATE N
			SET kCura_Import_Status = kCura_Import_Status | {(long)Relativity.MassImport.DTO.ImportStatus.ErrorDuplicateAssociatedObject},
            kCura_Import_ErrorData = D.ObjectName + '|{associatedObjectTable}' + '|@fieldName'
			FROM [Resource].[{tableNames.Native}] N
				JOIN [Resource].[{tableNames.Objects}] O
					ON N.[{keyFieldColumnName}] = O.DocumentIdentifier
				JOIN DuplicatedObjects D
					ON O.[ObjectName] = D.ObjectName
			WHERE O.[ObjectTypeId] = {associatedArtifactTypeID};
		";
		}

		public FormattableString SetArtifactIdForExistingMultiObjects(TableNames tableNames, string keyFieldColumnName, string associatedObjectTable, string associatedObjectIdentifierColumn, string associatedArtifactTypeID)
		{
			return $@"
			UPDATE O
			SET objectartifactid = MO.ArtifactID
			FROM [Resource].[{tableNames.Objects}] O 
				JOIN [Resource].[{tableNames.Native}] N
					ON O.DocumentIdentifier = N.[{keyFieldColumnName}]
				JOIN [{associatedObjectTable}] MO
					ON O.ObjectName = MO.[{associatedObjectIdentifierColumn}]
			WHERE [ObjectArtifactID] = -1
				AND O.[ObjectTypeId] = {associatedArtifactTypeID}
				AND N.kCura_Import_Status = 0;
		";
		}

		public string CreateNativeFileRows()
		{
			return $@"/*
		Format replace:
		---------------
		0: native temp table
										  1: inRepository flag
		*/

		INSERT INTO [File](
		[Guid],
		[DocumentArtifactID],
		[Filename],
		[Order],
		[Type],
		[Rotation],
		[Identifier],
		[Location],
		[Size],
		[InRepository],
		[Billable]
		)	SELECT
		[kCura_Import_FileGuid],
		[ArtifactID],
		[kCura_Import_Filename],
		0,
		0,
		-1,
		'DOC' + CAST([ArtifactID] AS VARCHAR(50)) + '_NATIVE',
		[kCura_Import_Location],
		[kCura_Import_FileSize],
		{{1}},
		{{2}}
		FROM
		[Resource].[{{0}}] tmp
		WHERE
		tmp.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
		AND
		NOT ISNULL([kCura_Import_FileGuid], '') = ''

		SELECT @@ROWCOUNT

		/*NativeImportAuditClause*/";
		}

		public InlineSqlQuery CreateAuditClauseWithEnabledExtractedText(TableNames tableNames, int fieldArtifactId, string details, string requestOrigination, string recordOrigination)
		{
			return new InlineSqlQuery($@"
/*
Parameters:
	@userID
*/
INSERT INTO AuditRecord (
	[ArtifactID],
	[Action],
	[Details],
	[UserID],
	[TimeStamp],
	[RequestOrigination],
	[RecordOrigination]
)	SELECT
		P.[ArtifactID],
		2 As [Action],
		{details},
		@userID As [UserID],
		GETUTCDATE() As [TimeStamp],
		'{requestOrigination}' AS [RequestOrigination],
		'{recordOrigination}' AS [RecordOrigination]
FROM
	[Resource].[{tableNames.Part}] P
	/* Do this inner clause only if extracted text is enabled */
	INNER JOIN [Resource].[{tableNames.Native}] N ON
	N.[kCura_Import_ID] = P.[kCura_Import_ID]
WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = {fieldArtifactId};
");
		}

		public string CheckForExistingNativeFiles()
		{
			return $@"/*
  Format replace:
  ---------------
  0: native temp table
*/
SELECT 1 WHERE EXISTS (
    SELECT 1 FROM [Resource].[{{0}}]
    WHERE [kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
    AND   EXISTS (
        SELECT 1 FROM EDDSDBO.[File] WHERE [Type] = 0 AND DocumentArtifactID = ArtifactID
    )
)
";
		}

		public string DeleteExistingNativeFiles()
		{
			return $@"/*
  Format replace:
  ---------------
  0: native temp table
*/
	DELETE
	FROM
		[File]
	/*NativeImportAuditIntoClause*/
	WHERE
		[Type] = 0
		AND
		EXISTS(
			SELECT
				ArtifactID
			FROM
				[Resource].[{{0}}]
			WHERE
				ArtifactID = [DocumentArtifactID]
				AND
				[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
		)
	";
		}

		public string AuditWrapper(string wrappedCall)
		{
			return $@"
INSERT INTO [AuditRecord_PrimaryPartition] (
	[ArtifactID],
	[Action],
	[Details],
	[UserID],
	[TimeStamp],
	[RequestOrigination],
	[RecordOrigination]
)
SELECT * FROM (
 /*AuditRecordWrappedCall*/
) AS entries
	".Replace("/*AuditRecordWrappedCall*/", wrappedCall);
		}

		public string FileFieldAuditJoin(string fileTable)
		{
			return $@"LEFT JOIN [{fileTable}] F ON
	F.ObjectArtifactID = N.[ArtifactID]";
		}

		public string MapFieldsAuditJoin(string auditMapClause, string mapTable)
		{
			return $@"LEFT JOIN (
	SELECT 
		M.ArtifactID{auditMapClause}
	FROM [Resource].[{mapTable}] M
	GROUP BY M.ArtifactID
) GM ON
	GM.ArtifactID = N.[ArtifactID]";
		}

		public string GetReturnReport()
		{
			return $@"/*
	Format replace:
	---------------
	0: Temp Table Name
	1: Artifact Part table Name
	2: Top Field Artifact ID
*/

SELECT
	NewDocument = SUM(CASE
		WHEN N.[kCura_Import_IsNew] = 1 THEN 1
		ELSE 0
	END),
	UpdatedDocument = SUM(CASE
		WHEN N.[kCura_Import_IsNew] = 0 THEN 1
		ELSE 0
	END),
	FileCount = SUM(CASE
		WHEN NOT ISNULL(N.[kCura_Import_FileGuid], '') = '' THEN 1
		ELSE 0
	END)
FROM
	[Resource].[{{0}}] N
WHERE
	N.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}";
		}

		public string GetDetailedReturnReport()
		{
			return $@"/*
	Format replace:
	---------------
	0: Temp Table Name
	1: Artifact Part table name
	2: Key Field Name
	3: Top Field Artifact ID
*/

SELECT
	[ArtifactID] = A.[ArtifactID],
	[KeyFieldName] = N.[{{2}}]
FROM
	[Resource].[{{0}}] N
JOIN 
	[Resource].[{{1}}] A
ON
	N.[kCura_Import_ID] = A.[kCura_Import_ID] AND A.[FieldArtifactID] = 0
WHERE
	N.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}";
		}

		public InlineSqlQuery InsertAssociatedObjects(TableNames tableNames, string associatedObjectTable, string idFieldColumnName, FieldInfo field)
		{
			return new InlineSqlQuery($@"
INSERT INTO [{associatedObjectTable}] (
	[ArtifactID],
	[{idFieldColumnName}]
)
SELECT DISTINCT
	P.ArtifactID,
	N.[{field.GetColumnName()}]
FROM
	[Resource].[{tableNames.Part}] P
INNER JOIN [Resource].[{tableNames.Native}] N ON
	N.[kCura_Import_ID] = P.[kCura_Import_ID] AND N.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = {field.ArtifactID};
");
		}

		public InlineSqlQuery InsertDataGridRecordMapping(TableNames tableNames, int fieldArtifactId)
		{
			return new InlineSqlQuery($@"
INSERT INTO [DataGridRecordMapping] (
	[DocumentArtifactID]
) SELECT
		[ArtifactID]
	FROM
		[Resource].[{tableNames.Part}] P
	WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = {fieldArtifactId};
");
		}

		public InlineSqlQuery InsertDocuments(string documentTable, string selectClause, string setClause, TableNames tableNames, string topFieldArtifactID)
		{
			return new InlineSqlQuery($@"
INSERT INTO [{documentTable}] (
	[ArtifactID],
	[AccessControlListID_D],
	[ParentArtifactID_D]{selectClause}
) SELECT
	P.[ArtifactID],
	PARENT.[ParentAccessControlListID],
	PARENT.[ParentArtifactID]{setClause}
FROM
	[Resource].[{tableNames.Native}] N
INNER JOIN [Resource].[{tableNames.Part}] P ON P.[kCura_Import_ID] = N.[kCura_Import_ID]
INNER JOIN [Resource].[{tableNames.Parent}] PARENT ON PARENT.[kCura_Import_ID] = N.[kCura_Import_ID]
WHERE
	N.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending} AND P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = {topFieldArtifactID};
");
		}

		public InlineSqlQuery InsertGenericObjects(string objectTableName, TableNames tableNames, string selectClause, string setClause, int fieldArtifactId)
		{
			return new InlineSqlQuery($@"
INSERT INTO [{objectTableName}] (
	[ArtifactID]{selectClause}
) SELECT
	P.ArtifactID{setClause}
FROM
	[Resource].[{tableNames.Native}] N
INNER JOIN [Resource].[{tableNames.Part}] P ON
	P.[kCura_Import_ID] = N.[kCura_Import_ID]
WHERE
	N.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending} AND P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = {fieldArtifactId};
");
		}

		public InlineSqlQuery InsertIntoCodeArtifactTableForDocImages(TableNames tableNames, string codeArtifactTableName, int fieldArtifactId)
		{
			return new InlineSqlQuery($@"
/*
Parameters:
	@hasImagesCodeArtifactID
*/
INSERT INTO [{codeArtifactTableName}] (
	[CodeArtifactID],
	[AssociatedArtifactID]
)	SELECT
		@hasImagesCodeArtifactID,
		ArtifactID
	FROM
		[Resource].[{tableNames.Part}] P
	WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = {fieldArtifactId}
");
		}

		public InlineSqlQuery OverwriteOnlyErrors(TableNames tableNames)
		{
			return new InlineSqlQuery($@"
UPDATE N
SET
	[kCura_Import_Status] = [kCura_Import_Status] + IIF(EXISTING_OBJECTS.CountValue IS NULL, {(long)Relativity.MassImport.DTO.ImportStatus.ErrorOverwrite}, {(long)ImportStatus.ErrorOverwriteMultipleKey})
FROM [Resource].[{tableNames.Native}] N
LEFT OUTER JOIN
(
	SELECT 
		P.kCura_Import_ID,
		COUNT(*) AS CountValue
	FROM [Resource].[{tableNames.Part}] P
	GROUP BY P.kCura_Import_ID
) AS EXISTING_OBJECTS
ON N.[kCura_Import_ID] = EXISTING_OBJECTS.[kCura_Import_ID]
WHERE EXISTING_OBJECTS.CountValue IS NULL OR EXISTING_OBJECTS.CountValue != 1;
");
		}

		public virtual string PopulateArtifactIdColumnOnTempTable(TableNames tableNames)
		{
			// TODO After switching to the part table for [ArtifactID] and [kCura_Import_IsNew] columns we can remove this UPDATE.
			return $@"/*
	Parameters:
	-----------
	@userID
*/

UPDATE
	N
SET
	N.[ArtifactID] = P.[ArtifactID],
	N.[kCura_Import_IsNew] = P.[kCura_Import_IsNew]
FROM
	[Resource].[{tableNames.Native}] N
INNER JOIN [Resource].[{tableNames.Part}] P ON
	P.[kCura_Import_ID] = N.[kCura_Import_ID] AND P.[FieldArtifactID] = 0
WHERE
	N.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending};
";
		}

		public string PopulateFileTables()
		{
			return $@"/*
	Parameters
	----------
	@ ObjectArtifactTypeID

	Format replace:
	---------------
	0: File Table Name
	1: temp table name
	2: file name column name
	3: file size column name
	4: file location column name
	5: main object table
	6: file field name
	7: Master database prepend
*/

DECLARE @rowsAffected INT SET @rowsAffected=1
DECLARE @CaseArtifactID int
Set @CaseArtifactID = (select substring (db_name(), 5, 50))
DECLARE @codeTypeID INT
SET @codeTypeID = (SELECT TOP 1 CodeTypeID FROM {{7}}[CodeType] WHERE [Name] = 'FileLocation')
DECLARE @dfiles TABLE ([Guid] NVARCHAR(1000), [Location] NVARCHAR(2000))

WHILE @rowsAffected > 0 BEGIN
	DELETE TOP (1000) FROM
		[{{0}}]
	OUTPUT
		'{{0}}' + '_' + CAST(DELETED.[FileID] AS NVARCHAR(30)),
		DELETED.[Location]
	INTO
		@dfiles
	WHERE EXISTS(
		SELECT ArtifactID
		FROM [Resource].[{{1}}]
		WHERE ArtifactID = [{{0}}].[ObjectArtifactID] AND [{{1}}].[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
	)
	SET @rowsAffected=@@ROWCOUNT

	INSERT INTO {{7}}[DeleteFile] ([ApplicationID], [Guid], [FileLocation])	SELECT
		[ApplicationID] = @CaseArtifactID,
		[Guid],
		[Location]
	FROM
		@dfiles D
	INNER JOIN {{7}}[Code] ON
		[Code].CodeTypeID = @codeTypeID
		AND
		D.[Location] COLLATE SQL_Latin1_General_CP1_CI_AS LIKE [Code].[name] + DB_NAME() + '\%' COLLATE SQL_Latin1_General_CP1_CI_AS

	DELETE FROM @dfiles
END

/*DeleteFileAuditClause*/

DECLARE @tbl TABLE ([FileID] INT , [ObjectArtifactID] INT )

INSERT INTO [{{0}}] (
	ObjectArtifactID,
	Filename,
	Size,
	Location,
	Status,
	Message
) OUTPUT
	INSERTED.[FileID],
	INSERTED.[ObjectArtifactID]
INTO
	@tbl
SELECT
	[ArtifactID],
	[{{2}}],
	[{{3}}],
	[{{4}}],
	0,
	''
FROM
	[Resource].[{{1}}]
WHERE
	NOT [{{2}}] IS NULL
	AND [{{1}}].[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}

UPDATE
	[{{5}}]
SET
	[{{5}}].[{{6}}] = T.[FileID],
	[{{5}}].[{{6}}Text] = ''
FROM
	[{{5}}]
LEFT JOIN @tbl T ON
	T.[ObjectArtifactID] = [{{5}}].[ArtifactID]
INNER JOIN [Resource].[{{1}}] ON
	[{{1}}].[ArtifactID] = [{{5}}].[ArtifactID]
	AND
	[{{1}}].[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}

SELECT @@ROWCOUNT

DECLARE @AppID INT SET @AppID = (SELECT CAST(REPLACE(DB_NAME(), 'EDDS', '') AS INT))
DECLARE @FileFieldArtifactID INT SET @FileFieldArtifactID = (SELECT CAST(REPLACE('{{0}}', 'File', '') AS INT))
INSERT INTO
	{{7}}TextExtractionQueue (
		ApplicationID,
		FileLocation,
		ObjectArtifactID,
		FileID,
		FileFieldArtifactID,
		TimeStamp,
		Priority,
		Processor,
		ObjectArtifactTypeID
	) SELECT
		@AppID,
		[{{1}}].[{{4}}],
		T.[ObjectArtifactID],
		T.[FileID],
		@FileFieldArtifactID,
		GETUTCDATE(),
		0,
		'',
		@ObjectArtifactTypeID
	FROM
		[Resource].[{{1}}]
	INNER JOIN @tbl T ON
		T.[ObjectArtifactID] = [{{1}}].[ArtifactID]
		AND
		[{{1}}].[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}

/*CreateFileAuditClause*/";
		}

		public string SetObjectsFieldEntries()
		{
			return $@"/*
	0: Objects Field Table
	1: ContainingObjectField
	2: NewObjectField
	3: Containing Object Identifier Column
	4: Objects Temp Table
	5: Native Temp Table
	6: field overlay switch statement

	@fieldID
	*/

IF {{6}} = 0
BEGIN
	DELETE
	FROM [{{0}}]
	OUTPUT DELETED.[{{1}}], DELETED.[{{2}}], @fieldID, 0 INTO [Resource].[{{7}}]
	WHERE
		[{{1}}] in (SELECT ArtifactID FROM [Resource].[{{5}}] WHERE kCura_Import_Status = {(long)Relativity.MassImport.DTO.ImportStatus.Pending})

	INSERT INTO [{{0}}] ([{{1}}],[{{2}}])
	OUTPUT INSERTED.[{{1}}], INSERTED.[{{2}}], @fieldID, 1 INTO [Resource].[{{7}}]
	SELECT DISTINCT
		ContainingArtifactID = [{{5}}].[ArtifactID],
		ObjectArtifactID
	FROM [Resource].[{{4}}] tmp
	INNER JOIN [Resource].[{{5}}] ON [{{5}}].[{{3}}] = tmp.DocumentIdentifier
	WHERE
		FieldID = @fieldID AND [{{5}}].[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
END
ELSE
BEGIN
	INSERT INTO [{{0}}] ([{{1}}],[{{2}}])
	OUTPUT INSERTED.[{{1}}], INSERTED.[{{2}}], @fieldID, 1 INTO [Resource].[{{7}}]
	SELECT DISTINCT
		ContainingArtifactID = [{{5}}].[ArtifactID],
		ObjectArtifactID
	FROM [Resource].[{{4}}] tmp
	INNER JOIN [Resource].[{{5}}] ON [{{5}}].[{{3}}] = tmp.DocumentIdentifier
  LEFT JOIN [{{0}}] AS [ExistingObjects] ON
		[ExistingObjects].[{{1}}] = [{{5}}].[ArtifactID]
    AND
	[ExistingObjects].[{{2}}] = [ObjectArtifactID]
	WHERE
		FieldID = @fieldID AND [{{5}}].[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
    AND
	[ExistingObjects].[{{1}}] IS NULL
	AND
	[ExistingObjects].[{{2}}] IS NULL
END

";
		}


		public string TextIdentifierColumn()
		{
			return "[{2}]";
		}

		public string KeyFieldColumn()
		{
			return "[{12}]";
		}

		public string TextIdentifierColumnForAssociatedObjects()
		{
			return "[{9}]";
		}

		public string UpdateAuditClauseInsert(string nativeTable, int action, string auditDetailsClause)
		{
			return $@"
INSERT INTO AuditRecord(
	[ArtifactID],
	[Action],
	[Details],
	[UserID],
	[TimeStamp],
	[RequestOrigination],
	[RecordOrigination]
) SELECT
	N.ArtifactID,
	{action},
	{auditDetailsClause}
	@auditUserID,
	GETUTCDATE(),
	@requestOrig,
	@recordOrig
FROM
	[Resource].[{nativeTable}] N
/* FileFieldAuditJoin */
/* MapFieldsAuditJoin */
WHERE
	N.[kCura_Import_IsNew] = 0
	AND
	N.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending};
";
		}




		public string UpdateAuditClauseInsertNew(string nativeTable, int action, string auditDetailsClause, bool useTempTable)
		{
			auditDetailsClause = auditDetailsClause != string.Empty ? auditDetailsClause : "''";

			if (useTempTable)
			{
				return $@"
INSERT INTO #TempAudit (
	[ArtifactID],
	[Details]
) SELECT
	N.ArtifactID,
	{auditDetailsClause}
FROM
	[Resource].[{nativeTable}] N
/* FileFieldAuditJoin */
/* MapFieldsAuditJoin */
WHERE
	N.[kCura_Import_IsNew] = 0
	AND
	N.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending};
";
			}
			else
			{
				return $@"
INSERT INTO AuditRecord_PrimaryPartition (
	[ArtifactID],
	[Action],
	[Details],
	[UserID],
	[TimeStamp],
	[RequestOrigination],
	[RecordOrigination]
) SELECT
	N.ArtifactID,
	{action},
	CAST(N'<auditElement>' AS NVARCHAR(MAX)) + {auditDetailsClause} + '</auditElement>',
	@auditUserID,
	GETUTCDATE(),
	@requestOrig,
	@recordOrig
FROM
	[Resource].[{nativeTable}] N
/* FileFieldAuditJoin */
/* MapFieldsAuditJoin */
WHERE
	N.[kCura_Import_IsNew] = 0
	AND
	N.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending};
";
			}
		}

		public string UpdateAuditClauseMerge(int action, string auditDetailsClause)
		{
			return $@"
OUTPUT
	N.ArtifactID,
	{action},
	{auditDetailsClause}
	@auditUserID,
	GETUTCDATE(),
	@requestOrig,
	@recordOrig,
	NULL,
	NULL
INTO
	AuditRecord_PrimaryPartition
";
		}

		public string UpdateAuditClauseMergeNew(int action, string auditDetailsClause, bool useTempTable)
		{
			auditDetailsClause = auditDetailsClause != string.Empty ? auditDetailsClause : "''";
			auditDetailsClause = auditDetailsClause.Remove(auditDetailsClause.Length - 1);
			if (useTempTable)
			{
				return $@"
OUTPUT
	N.ArtifactID,
	{auditDetailsClause}
INTO
	#TempAudit
";
			}
			else
			{
				return $@"
OUTPUT
	N.ArtifactID,
	{action},
	CAST(N'<auditElement>' AS NVARCHAR(MAX)) + {auditDetailsClause} + '</auditElement>',
	@auditUserID,
	GETUTCDATE(),
	@requestOrig,
	@recordOrig,
	NULL,
	NULL
INTO
	AuditRecord_PrimaryPartition
";

			}
		}

		public string UpdateMetadata()
		{
			return $@"/*
	Parameters:
	-----------
	@userID
	@auditUserID
	@requestOrig
	@recordOrig

	Format replace:
	---------------
	0: native temp table
	1: set clause
	2: audit details clause
	3: artifact type table name
*/

/* UpdateAuditRecordsInsert */

/* UpdateObjectOrDocTable */

/* UpdateMismatchedFields */

UPDATE
	A
SET
	[LastModifiedOn] = GETUTCDATE(),
	[LastModifiedBy] = @auditUserID,
	[ParentArtifactID] = CASE
		WHEN N.[kCura_Import_ParentFolderID] = -1 OR A.[ArtifactTypeID] = {(int)ArtifactType.Document} THEN [ParentArtifactID]
		ELSE N.[kCura_Import_ParentFolderID]
	END
	/* UpdateTextIdentifier */
FROM
	[Artifact] A
INNER JOIN [Resource].[{{0}}] N ON
	A.[ArtifactID] = N.[ArtifactID]
WHERE
	N.[kCura_Import_IsNew] = 0
	AND
	N.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}

SELECT @@ROWCOUNT
				   ";
		}



		public string UpdateMetadataNew()
		{
			return $@"/*
	Parameters:
	-----------
	@userID
	@auditUserID
	@requestOrig
	@recordOrig

	Format replace:
	---------------
	0: native temp table
	1: set clause
	2: audit details clause
	3: artifact type table name
*/

/* UpdateAuditRecordsInsert */

/* UpdateObjectOrDocTable */

SELECT @@ROWCOUNT";
		}

		public string UpdateArtifactTableForOverlaidRecords(string nativeTempTableName, string updateTextIdentifierValue)
		{
			return $@"/*
	Parameters:
	-----------
	@auditUserID
*/

UPDATE
	A
SET
	[LastModifiedOn] = GETUTCDATE(),
	[LastModifiedBy] = @auditUserID,
	[ParentArtifactID] = CASE
		WHEN N.[kCura_Import_ParentFolderID] = -1 OR A.[ArtifactTypeID] = {(int)ArtifactType.Document} THEN [ParentArtifactID]
		ELSE N.[kCura_Import_ParentFolderID]
	END
	{updateTextIdentifierValue}
FROM
	[Artifact] A
INNER JOIN [Resource].[{nativeTempTableName}] N ON
	A.[ArtifactID] = N.[ArtifactID]
WHERE
	N.[kCura_Import_IsNew] = 0
	AND
	N.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}


		SELECT @@ROWCOUNT";
		}

		public string CreateTempTableForOverlayAudit()
		{
			return @"
CREATE TABLE #TempAudit(
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[ArtifactID] [int] NOT NULL,
	[Details] [nvarchar](max) NOT NULL

	INDEX IX_ArtifactID_ID CLUSTERED 
	(
		[ArtifactID] ASC,
		[ID] ASC
	)
)";
		}

		public string CopyRecordsFromTempAuditToAudit(int action)
		{
			return $@"
/*
	Parameters:
	-----------
	@auditUserID
	@requestOrig
	@recordOrig
*/

INSERT INTO AuditRecord_PrimaryPartition
SELECT
	[ArtifactID],
	{action},
	CAST(N'<auditElement>' AS NVARCHAR(MAX)) + STRING_AGG([Details], '') WITHIN GROUP(ORDER BY ID ASC) + '</auditElement>',
	@auditUserID,
	GETUTCDATE(),
	@requestOrig,
	@recordOrig,
	NULL,
	NULL
FROM #TempAudit
GROUP BY [ArtifactID]

SELECT @@ROWCOUNT";
		}


		public string UpdateObjectOrDocTable()
		{
			return $@"
UPDATE
	D
SET
{{1}}
/* UpdateAuditRecordsMerge */
FROM
	[{{3}}] D
INNER JOIN [Resource].[{{0}}] N ON  N.[ArtifactID] = D.[ArtifactID]
INNER JOIN [Resource].[{{4}}] P ON P.[kCura_Import_ID] = N.[kCura_Import_ID] AND P.[kCura_Import_IsNew] = 0 AND P.[FieldArtifactID] = {{5}}
/* FileFieldAuditJoin */
/* MapFieldsAuditJoin */
WHERE
	N.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending};
";
		}

		public string DoesColumnExistOnDocumentTable()
		{
			return @"
EXEC('SELECT COLUMN_NAME
	FROM   INFORMATION_SCHEMA.COLUMNS
	WHERE  TABLE_NAME = ''Document'' AND COLUMN_NAME IN (' + @columnNames + ')')";
		}

		public string UpdateMismatchedDataGridFields(string tempTableName, string setClause)
		{
			return $@"
	UPDATE [Document]
SET
	{setClause}
FROM
	[Document]
INNER JOIN [Resource].[{tempTableName}] tmp ON
	[Document].[ArtifactID] = tmp.[ArtifactID]
WHERE
	tmp.[kCura_Import_IsNew] = 0
	AND
	tmp.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}";
		}

		public InlineSqlQuery UpdateOverlayPermissions(TableNames tableNames, int artifactTypeId, int userID, int topFieldArtifactID)
		{
			return new InlineSqlQuery($@"; WITH EditList(EditAcl) AS (
	SELECT DISTINCT AccessControlListID
	FROM AccessControlListPermission
	WHERE
		PermissionID = (SELECT TOP 1 ArtifactTypePermission.PermissionID FROM ArtifactTypePermission INNER JOIN Permission ON Permission.PermissionID = ArtifactTypePermission.PermissionID AND [Type] = 2 AND ArtifactTypeID = {artifactTypeId})
		AND
		EXISTS(
			SELECT GroupArtifactID FROM GroupUser WHERE GroupArtifactID = AccessControlListPermission.GroupID AND UserArtifactID = {userID}
		)
)

UPDATE
	N
SET
	[kCura_Import_Status] = [kCura_Import_Status] + {(long)Relativity.MassImport.DTO.ImportStatus.SecurityUpdate}
FROM [Resource].[{tableNames.Native}] N
	JOIN [Resource].[{tableNames.Part}] P ON N.kCura_Import_ID = P.kCura_Import_ID
WHERE
	NOT P.[kCura_Import_IsNew] = 1
	AND N.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
	AND P.[FieldArtifactID] = {topFieldArtifactID}
	AND NOT EXISTS
	(
		SELECT AccessControlListID 
		FROM [Resource].[{tableNames.Part}] P
		JOIN EditList 
			ON EditList.EditAcl = P.[AccessControlListID] 
			AND P.[kCura_Import_ID] = N.[kCura_Import_ID]
	);
	");
		}

		public virtual string VerifyExistenceOfAssociatedMultiObjects(TableNames tableNames, string importedIdentifierColumn, string idFieldColumnName, string associatedObjectTable, FieldInfo field)
		{
			return $@"/*craete errors for associated multi objects that do not exist*/
UPDATE N
SET 
	[kCura_Import_Status] = [kCura_Import_Status] + {(long)Relativity.MassImport.DTO.ImportStatus.ErrorAssociatedObjectIsMissing},
	[kCura_Import_ErrorData] = '@fieldName|' + N.[{field.GetColumnName()}] + '|{associatedObjectTable}'
FROM [Resource].[{tableNames.Native}] N
WHERE N.[{importedIdentifierColumn}] IN (SELECT [{tableNames.Native}].[{importedIdentifierColumn}]
	FROM [Resource].[{tableNames.Native}] INNER JOIN
		[Resource].[{tableNames.Objects}] ON [{tableNames.Objects}].[DocumentIdentifier] = [{tableNames.Native}].[{importedIdentifierColumn}]
	WHERE NOT EXISTS(SELECT [{idFieldColumnName}] FROM [{associatedObjectTable}] WHERE [{tableNames.Objects}].ObjectArtifactID = [{associatedObjectTable}].[ArtifactID])
	AND [{tableNames.Objects}].FieldID = {field.ArtifactID}
	AND [{tableNames.Native}].[{field.GetColumnName()}] IS NOT NULL
	AND [kCura_Import_Status] {"& "}{(long)Relativity.MassImport.DTO.ImportStatus.ErrorAssociatedObjectIsMissing} = 0)
";
		}


		public InlineSqlQuery InsertSelfReferencedObjects(TableNames tableNames, string idFieldColumnName, FieldInfo singleObjectField, int parentAccessControlListId)
		{
			return new InlineSqlQuery($@"
INSERT INTO [Resource].[{tableNames.Part}]
SELECT
		N2.[kCura_Import_Id],
		0 [kCura_Import_IsNew],
		P.[ArtifactID],
		{parentAccessControlListId},
		0 [FieldArtifactID]
FROM [Resource].[{tableNames.Native}] N 
		JOIN [Resource].[{tableNames.Part}] P 
		ON N.[kCura_Import_ID] = P.[kCura_Import_ID] 
		JOIN [Resource].[{tableNames.Native}] N2
		ON N.[{singleObjectField.GetColumnName()}] = N2.[{idFieldColumnName}]
WHERE P.[FieldArtifactID] = {singleObjectField.ArtifactID}
		AND P.[kCura_Import_IsNew] = 1
");
		}

		public string InsertMultiSelfReferencedObjects(string nativeTableName, string partTableName, string idFieldColumnName, int parentAccessControlListId)
		{
			return $@"
INSERT INTO [Resource].[{partTableName}]
SELECT
	N.[kCura_Import_Id],
	0 [kCura_Import_IsNew],
	P.[ArtifactID],
	{parentAccessControlListId},
	0 [FieldArtifactID]
FROM [Resource].[{nativeTableName}] N
	JOIN #InsertedArtifactIDsTable P
		ON N.[{idFieldColumnName}] = P.[ObjectName];
";
		}

		public InlineSqlQuery ValidateIdentifierIsNonNull(TableNames tableNames, string identifierColumnName)
		{
			return ValidateColumnIsNonNull(tableNames, identifierColumnName, (long)Relativity.MassImport.DTO.ImportStatus.EmptyIdentifier);
		}

		public InlineSqlQuery ValidateOverlayIdentifierIsNonNull(TableNames tableNames, string overlayIdentifierColumnName)
		{
			return ValidateColumnIsNonNull(tableNames, overlayIdentifierColumnName, (long)Relativity.MassImport.DTO.ImportStatus.EmptyOverlayIdentifier);
		}

		private InlineSqlQuery ValidateColumnIsNonNull(TableNames tableNames, string columnName, long errorCode)
		{
			return new InlineSqlQuery($@"
UPDATE [Resource].[{tableNames.Native}]
SET [kCura_Import_Status] = [kCura_Import_Status] | {errorCode}
WHERE [{columnName}] IS NULL;");
		}
	}
}