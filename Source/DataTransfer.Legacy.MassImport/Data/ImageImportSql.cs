namespace Relativity.MassImport.Data
{
	internal class ImageImportSql
	{
		/// <summary>
		/// Format replace:
		/// ---------------
		/// 0: img temp table
		/// 1: identifier column name
		/// </summary>
		public string AppendOnlyErrors()
		{
			return $@"
UPDATE
	[Resource].[{{0}}]
SET
	[Status] = [Status] + {(long)Relativity.MassImport.DTO.ImportStatus.ErrorAppend}
WHERE
	EXISTS(SELECT [ArtifactID] FROM [Document] WHERE [Document].[{{1}}] = [{{0}}].[DocumentIdentifier])";
		}

		/// <summary>
		/// Format replace:
		/// ---------------
		/// 0: img temp table
		/// 1: identifier column name
		/// </summary>
		public string BatesExistsErrors()
		{
			return $@"
WITH ErrorRows AS (

SELECT DISTINCT
	kCura_Status = CASE
		WHEN
			[Document].[ArtifactID] IS NULL
		THEN
			{(long)Relativity.MassImport.DTO.ImportStatus.ErrorBates}

		WHEN
			NOT [Document].[ArtifactID] IS NULL
			AND
			[Document].[{{1}}] NOT IN (SELECT [DocumentIdentifier] FROM [Resource].[{{0}}])
		THEN
			{(long)Relativity.MassImport.DTO.ImportStatus.ErrorBates}

		WHEN
			NOT [Document].[ArtifactID] IS NULL
			AND
			[Document].[{{1}}] IN (SELECT [DocumentIdentifier] FROM [Resource].[{{0}}])
			AND
			NOT [Document].[ArtifactID] = [File].[DocumentArtifactID]
		THEN
			{(long)Relativity.MassImport.DTO.ImportStatus.ErrorBates}
	END,
	[{{0}}].[DocumentIdentifier]
FROM
	[Resource].[{{0}}]
LEFT JOIN eddsdbo.[Document] ON
	[Document].[{{1}}] = [{{0}}].[DocumentIdentifier]
INNER JOIN [Resource].[{{0}}_ExistingFile] [File] ON
	[File].[FileIdentifier] = [{{0}}].[FileIdentifier]
WHERE
	[{{0}}].[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
)

UPDATE
	[Resource].[{{0}}]
SET
	[Status] = [Status] + kCura_Status
FROM
	[Resource].[{{0}}]
INNER JOIN ErrorRows ON
	NOT [ErrorRows].kCura_Status IS NULL
	AND
	[ErrorRows].[DocumentIdentifier] = [{{0}}].[DocumentIdentifier]";
		}

		public string CreateAuditClause()
		{
			return $@"INSERT INTO AuditRecord (
	[ArtifactID],
	[Action],
	[Details],
	[UserID],
	[TimeStamp],
	[RequestOrigination],
	[RecordOrigination]
)	SELECT
		ArtifactID,
		2,
		/* DetailsValueColumnName */,
		@userID,
		GETUTCDATE(),
		'{{5}}',
		'{{6}}'
	FROM
		[Resource].[{{1}}]";
		}

		/// <summary>
		/// Format:
		/// -------
		/// 0: table name
		/// 1: document identifier column collation
		/// 2: document identifier unicode marker (either N or an empty string)
		/// </summary>
		public string CreateImageArtifactsTableFormatString()
		{
			return $@"
IF NOT EXISTS(SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{{0}}')
BEGIN
	CREATE TABLE [Resource].[{{0}}](
		[ArtifactID] INT NOT NULL,
		[ParentArtifactID] INT NOT NULL,
		[TextIdentifier] {{2}}VARCHAR(500) COLLATE {{1}}
	)
END
ELSE
BEGIN
	TRUNCATE TABLE [Resource].[{{0}}]
END";
		}

		/// <summary>
		/// Format replace:
		/// ---------------
		/// 0: img temp table
		/// 1: InRepository
		/// 2: Billable
		/// 3: Type: 1 - image, 6 - pdf
		/// </summary>
		public string CreateImageFileRows()
		{
			return $@"
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
		[Guid],
		[ArtifactID],
		[Filename],
		[Order],
		{{3}},
		-1,
		[FileIdentifier],
		[Location],
		[FileSize],
		{{1}},
		{{2}}
	FROM
		[Resource].[{{0}}] tmp
	WHERE
		tmp.[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}

/*ImageInsertAuditRecords*/";
		}

		/// <summary>
		/// Format:
		/// -------
		/// 0: table name
		/// 1: document identifier column unicode marker
		/// 2: document identifier column width
		/// 3: document identifier column collation
		/// 4: file identifier column collation
		/// 5: full text encoding page column definition
		/// 6: full text column definition
		/// </summary>
		public string CreateImageTableFormatString()
		{
			return $@"
DECLARE @newJob AS BIT;

IF NOT EXISTS(SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = 'Resource' AND [TABLE_NAME] = '{{0}}')
BEGIN
	SET @newJob = 1;

	CREATE TABLE [Resource].[{{0}}](
		[kCura_Import_ID] INT IDENTITY(1,1) NOT NULL,
		[Status] BIGINT NOT NULL,
		[IsNew] BIT NOT NULL,
		[ArtifactID] INT NOT NULL,
		[OriginalLineNumber] INT NOT NULL,
		[DocumentIdentifier] {{1}}VARCHAR({{2}}) COLLATE {{3}} NOT NULL,
		[FileIdentifier] VARCHAR(255) COLLATE {{4}} NOT NULL,
		[Guid] VARCHAR(100) NOT NULL,
		[Filename] NVARCHAR(200) NOT NULL,
		[Order] INT NOT NULL,
		[Offset] INT NOT NULL,
		[Filesize] BIGINT NOT NULL,
		[Location] NVARCHAR(2000),
		[OriginalFileLocation] NVARCHAR(2000),
		[kCura_Import_DataGridException] NVARCHAR(MAX),
		{{5}}
		{{6}}
		CONSTRAINT [PK_{{0}}] PRIMARY KEY CLUSTERED (
			[kCura_Import_ID] ASC
		)
	)
END
ELSE
BEGIN
	SET @newJob = 0;
	TRUNCATE TABLE [Resource].[{{0}}]
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Resource].[{{0}}]') AND name = N'IX_LookupFast')
BEGIN
	CREATE NONCLUSTERED INDEX [IX_LookupFast] ON [Resource].[{{0}}]
	(
		[FileIdentifier] ASC
	)
END

SELECT @newJob;
";
		}

		/// <summary>
		/// Parameters:<br />
		/// -----------
		/// @parentArtifactID
		/// @userID
		/// 
		/// Format replace:
		/// ---------------
		/// 0: img temp table
		/// 1: artifact tmp table
		/// 2: identifier column name
		/// 3: relational and relational index column names
		/// 4: relational and relational index set values (loops of textidentifier and -1s
		/// 5: request origination
		/// 6: record origination
		/// </summary>
		public string CreateNewDocumentsFromImageLoad()
		{
			return $@"
TRUNCATE TABLE [Resource].[{{1}}]
DECLARE @HasPermissionToAdd BIT
SET @HasPermissionToAdd = CASE
	WHEN EXISTS(
		SELECT [GroupID]
		FROM AccessControlListPermission
		WHERE
			PermissionID = 43
			AND
			AccessControlListID = (SELECT TOP 1 AccessControlListID FROM [Artifact] WHERE ArtifactID = @parentArtifactID)
			AND
			EXISTS(SELECT GroupArtifactID FROM GroupUser WHERE GroupArtifactID = GroupID AND UserArtifactID = @userID)
		) THEN 1 ELSE 0 END

/* UpdateOverlayPermissionsForAppendOverlayMode */

IF @HasPermissionToAdd > 0 BEGIN
DECLARE @aclID INT SET @aclID = (SELECT TOP 1 AccessControlListID FROM Artifact WHERE ArtifactID = @parentArtifactID)
DECLARE @containerID INT SET @containerID = (SELECT DISTINCT TOP 1 ContainerID FROM Artifact WHERE ARtifactTypeID = {(int) ArtifactType.Document})
DECLARE @now DATETIME SET @now = GETUTCDATE()
DECLARE @hasImagesCodeTypeID INT SET @hasImagesCodeTypeID = (SELECT TOP 1 CodeTypeID FROM CodeType WHERE [Name] = 'HasImages')
DECLARE @hasImagesCodeArtifactID INT SET @hasImagesCodeArtifactID = (SELECT TOP 1 [ArtifactID] FROM [Code] WHERE [Name]='Yes' AND CodeTypeID = @hasImagesCodeTypeID )
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
	) OUTPUT
		INSERTED.ArtifactID,
		INSERTED.ParentArtifactID,
		INSERTED.TextIdentifier
	INTO
		[Resource].[{{1}}]
	SELECT DISTINCT
		[ArtifactTypeID] = {(int) ArtifactType.Document},
		[ParentArtifactID] = @parentArtifactID,
		[AccessControlListID] = @aclID,
		[AccessControlListIsInherited] = 1,
		[CreatedOn] = @now,
		[LastModifiedOn] = @now,
		[CreatedBy] = @userID,
		[LastModifiedBy] = @userID,
		[TextIdentifier] = DocumentIdentifier,
		[ContainerID] = @containerID,
		[Keywords] = '',
		[Notes] = '',
		[DeleteFlag] = 0
	FROM [Resource].[{{0}}] tmp
	LEFT JOIN [Document] ON [Document].[{{2}}] = DocumentIdentifier
	WHERE [Document].[ArtifactID] IS NULL AND tmp.[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}

INSERT INTO [Document] (
	[ArtifactID],
	[{{2}}],
	[AccessControlListID_D],
	[ParentArtifactID_D],
{{3}}
	[HasNative]
) SELECT
	ArtifactID,
	TextIdentifier,
	@aclID,
	@parentArtifactID,
{{4}}
	0
FROM
	[Resource].[{{1}}] N

INSERT INTO ArtifactAncestry(
	ArtifactID,
	AncestorArtifactID
)	SELECT
		N.ArtifactID, AncestorArtifactID
	FROM
		[Resource].[{{1}}] N
	INNER JOIN ArtifactAncestry ON ArtifactAncestry.ArtifactID = ParentArtifactID
	UNION
	SELECT
		N.ArtifactID, AncestorArtifactID = N.ParentArtifactID
	FROM
		[Resource].[{{1}}] N

/* InsertAuditRecords */

INSERT INTO [DataGridRecordMapping] (
	[DocumentArtifactID]
) SELECT
		[ArtifactID]
	FROM
		[Resource].[{{1}}] N
END

 /* HasPermissionsToAddCheck */";
		}

		/// <summary>
		/// Parameters:
		/// -----------
		/// @prodID
		/// @prodIdXml
		/// 
		/// Format replace:
		/// ---------------
		/// 0: img temp table
		/// 1: inRepository Flag
		/// 2: isBillable Flag
		/// 3: Type: 3 - image, 8 - pdf
		/// </summary>
		public string CreateProductionImageFileRows()
		{
			return $@"
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
	[Details],
	[Billable]
)	SELECT
		[Guid],
		[ArtifactID],
		[Filename],
		[Order],
		{{3}},
		-1,
		LEFT(CAST(@prodID AS NVARCHAR(50)) + '_' + [FileIdentifier], 255),
		[Location],
		[FileSize],
		{{1}},
		@prodIdXml,
		{{2}}
	FROM
		[Resource].[{{0}}] tmp
	WHERE
		tmp.[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}

/*ImageInsertAuditRecords*/";
		}

		public string CreateWhenExtractedTextIsEnabledAuditClause()
		{
			return $@"
;WITH ImportExtractedText AS (
	SELECT
		[DocumentIdentifier],
		[ExtractedTextEncodingPageCode]
	FROM
		[Resource].[{{0}}]
	WHERE
		[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
		AND
		ISNULL([ExtractedTextEncodingPageCode], '-1') <> '-1'
)

INSERT INTO AuditRecord (
	[ArtifactID],
	[Action],
	[Details],
	[UserID],
	[TimeStamp],
	[RequestOrigination],
	[RecordOrigination]
)	SELECT
	N.ArtifactID,
	2,
  /* FullTextOverlayDetail */,
	@userID,
	GETUTCDATE(),
	'{{5}}',
	'{{6}}'
FROM
	[Resource].[{{1}}] N
/* Do this inner clause only if extracted text is enabled */
LEFT JOIN [ImportExtractedText] T ON
	T.[DocumentIdentifier] = N.[TextIdentifier]
	AND
	T.[ExtractedTextEncodingPageCode] IS NOT NULL";
		}

		/// <summary>
		/// Format replace:
		/// ---------------
		/// 0: img temp table
		/// 1: Type: 1 - image, 6 - pdf
		/// </summary>
		public string DeleteExistingImageFiles()
		{
			return $@"
DECLARE @rowsAffected INT SET @rowsAffected = 1
WHILE @rowsAffected > 0 BEGIN

	DELETE FROM
		[File]
	WHERE
		[Guid] IN (
			SELECT TOP 1000 [Guid]
			FROM [File]
			WHERE [Type] = {{1}}
				AND
				EXISTS(SELECT ArtifactID FROM [Resource].[{{0}}] WHERE ArtifactID = [DocumentArtifactID] AND [Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending})
		)
	SET @rowsAffected = @@ROWCOUNT
END

/*ImageInsertAuditRecords*/";
		}

		/// <summary>
		/// Format:
		/// -------
		/// 0: img temp table name
		/// 1: document keyField column name
		/// </summary>
		public string ExistingFilesLookupInitialization()
		{
			return $@"
SELECT
	[FileIdentifier] = [File].[Identifier],
	[DocumentArtifactID] = [File].[DocumentArtifactID]
INTO
	[Resource].[{{0}}_ExistingFile]
FROM
	EDDSDBO.[File]
INNER JOIN [Resource].[{{0}}]
  ON [File].[Identifier] COLLATE database_default = [{{0}}].FileIdentifier COLLATE database_default

IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[Resource].[{{0}}_ExistingFile]') AND name = N'IX_LookupFast')
DROP INDEX [IX_LookupFast] ON [Resource].[{{0}}_ExistingFile]

CREATE CLUSTERED INDEX [IX_LookupFast] ON [Resource].[{{0}}_ExistingFile]
(
    [FileIdentifier] ASC
)


		";
		}

		/// <summary>
		/// Format replace:
		/// ---------------
		/// 0: img temp table
		/// </summary>
		public string GetDocumentsToUpdate()
		{
			return "SELECT [ArtifactID] FROM [Resource].[{0}]";
		}

		public string GetErrors()
		{
			return $@"SELECT
		[{{0}}].OriginalLineNumber,
		[{{0}}].DocumentIdentifier,
		[{{0}}].FileIdentifier,
		[{{0}}].[FileName],
		[{{0}}].[Order],
		[{{0}}].[Status],
		[{{0}}].OriginalFileLocation,
		ExistingDocumentIdentifier = ISNULL(Document.[{{1}}], ''),
		ExistingDocumentArtifactID = ISNULL(EF.[DocumentArtifactID], -1),
		[{{0}}].[kCura_Import_DataGridException]
	FROM
		[Resource].[{{0}}] tmp
	LEFT JOIN [Resource].[{{0}}_ExistingFile] EF ON
		EF.FileIdentifier COLLATE SQL_Latin1_General_CP1_CI_AS = [Resource].[{{0}}].[FileIdentifier] COLLATE SQL_Latin1_General_CP1_CI_AS
	LEFT JOIN [EDDSDBO].[Document] Document ON
		Document.ArtifactID = EF.[DocumentArtifactID] 
	WHERE
		NOT tmp.[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
	ORDER BY
		[OriginalLineNumber]

	DROP TABLE [Resource].[{{0}}_ExistingFile]";
		}

		public string GetReturnReport()
		{
			return $@"DECLARE @docCreateCount INT
SET @docCreateCount = ISNULL((SELECT COUNT(DISTINCT [DocumentIdentifier]) FROM [Resource].[{{0}}] WHERE EXISTS(SELECT TOP 1 [ArtifactID] FROM [Resource].[{{1}}] WHERE [Resource].[{{0}}].[ArtifactID] = [{{1}}].[ArtifactID]) AND [Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending} AND NOT ArtifactID = 0), 0)

DECLARE @docUpdateCount INT
SET @docUpdateCount = ISNULL((SELECT COUNT(DISTINCT [DocumentIdentifier]) FROM [Resource].[{{0}}] WHERE NOT EXISTS(SELECT TOP 1 [ArtifactID] FROM [Resource].[{{1}}] WHERE [Resource].[{{0}}].[ArtifactID] = [{{1}}].[ArtifactID]) AND [Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending} AND NOT ArtifactID = 0), 0)

DECLARE @fileCount INT
SET @fileCount = ISNULL((SELECT COUNT([Guid]) FROM [Resource].[{{0}}] WHERE [Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending} AND NOT ArtifactID = 0 AND NOT ISNULL([Guid], '') = '' ), 0)

SELECT
	NewDocument = @docCreateCount,
	UpdatedDocument = @docUpdateCount,
	FileCount = @fileCount";
		}

		public string InsertImageCreationAuditRecords()
		{
			return @"INSERT INTO AuditRecord (
	[ArtifactID],
	[Action],
	[Details],
	[UserID],
	[TimeStamp],
	[RequestOrigination],
	[RecordOrigination]
)	SELECT
		ArtifactID,
		13,
		'',
		@userID,
		GETUTCDATE(),
		'{5}',
		'{6}'
	FROM
		[Resource].[{1}]";
		}

		public string InsertImageTableFormatString()
		{
			return @"INSERT INTO [Resource].[{0}] (
	[Status],
	[IsNew],
	[ArtifactID],
	[OriginalLineNumber],
	[DocumentIdentifier],
	[FileIdentifier],
	[Guid],
	[Filename],
	[Order],
	[Offset],
	[Filesize],
	[Location],
	[OriginalFileLocation],
	[FullText]
) VALUES (
	@Status,
	@IsNew,
	0,
	@OriginalLineNumber,
	@DocumentIdentifier,
	@FileIdentifier,
	@Guid,
	@Filename,
	@Order,
	@Offset,
	@Filesize,
	@Location,
	@OriginalFileLocation,
	NULL
)";
		}


		/// <summary>
		/// Format replace:
		/// ---------------
		/// 0: img temp table
		/// 1: CodeArtifact partition table name for HasImages
		/// </summary>
		public string ManageHasImagesForProductionImport()
		{
			return $@"
		DECLARE @hasImagesCodeArtifactID INT SET @hasImagesCodeArtifactID = (SELECT TOP 1 [ArtifactID] FROM [Code] JOIN [CodeType] ON [Code].[CodeTypeID] = [CodeType].[CodeTypeID] WHERE [Code].[Name]= 'No' AND [CodeType].[Name] = 'HasImages')

		INSERT INTO [{{1}}] (
		[CodeArtifactID],
		[AssociatedArtifactID]
		) SELECT
			@hasImagesCodeArtifactID,
			ArtifactID
			FROM
			[Document]
			WHERE
			EXISTS(
				SELECT
					ArtifactID 
				FROM [Resource].[{{0}}]
				WHERE [{{0}}].ArtifactID = [Document].[ArtifactID] AND [{{0}}].[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
				AND NOT EXISTS (
							SELECT 1
							FROM [{{1}}]
							WHERE [{{1}}].[AssociatedArtifactID] = [{{0}}].[ArtifactID]
								)
					)
";
		}


		/// <summary>
		/// Format replace:
		/// ---------------
		/// 0: img temp table
		/// 1: CodeArtifact partition table name for HasImages or HasPdf
		/// 2: CodeType Name: HasImages or HasPDF
		/// </summary>
		public string ManageHasImagesOrHasPDF()
		{
			return $@"
		DECLARE @hasImagesCodeArtifactID INT SET @hasImagesCodeArtifactID = (SELECT TOP 1 [ArtifactID] FROM [Code] JOIN [CodeType] ON [Code].[CodeTypeID] = [CodeType].[CodeTypeID] WHERE [Code].[Name]= 'Yes' AND [CodeType].[Name] = '{{2}}')

		DELETE FROM
		[{{1}}]
		WHERE
		EXISTS(
			SELECT
			ArtifactID
			FROM
			[Resource].[{{0}}]
			WHERE
			[{{0}}].ArtifactID = [{{1}}].[AssociatedArtifactID]
			AND
			[{{0}}].[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
		)

		INSERT INTO [{{1}}] (
		[CodeArtifactID],
		[AssociatedArtifactID]
		) SELECT
			@hasImagesCodeArtifactID,
			ArtifactID
			FROM
			[Document]
			WHERE
			EXISTS(
				SELECT ArtifactID FROM [Resource].[{{0}}] WHERE [{{0}}].ArtifactID = [Document].[ArtifactID] AND [{{0}}].[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending})
";
		}

		/// <summary>
		/// Format replace:
		/// ---------------
		/// 0: img temp table
		/// 1: artifact tmp table
		/// 2: extractedTextColumnName
		/// </summary>
		public string ManageImageFullText()
		{
			return $@"
UPDATE
	[Document]
SET
	[Document].[ExtractedText] = tmp.FullText
FROM
	[Document]
INNER JOIN [Resource].[{{0}}] tmp ON
	tmp.ArtifactID = [Document].[ArtifactID]
	AND
	NOT tmp.[FullText] IS NULL
	AND
	tmp.[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}

DELETE FROM
	[DocumentPage]
WHERE
	EXISTS(SELECT [ArtifactID] FROM [Resource].[{{0}}] N WHERE N.ArtifactID = [DocumentPage].[DocumentArtifactID] AND NOT N.[FullText] IS NULL AND N.[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending})

INSERT INTO [DocumentPage] (
	[DocumentArtifactID],
	[PageID],
	[ByteRange]
)	SELECT
		[ArtifactID],
		[Order],
		[Offset]
	FROM
		[Resource].[{{0}}] tmp
	WHERE
		NOT tmp.[FullText] IS NULL
		AND
		tmp.[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}";
		}

		/// <summary>
		/// Format replace:
		/// ---------------
		/// 0: img temp table
		/// 1: identifier column name
		/// </summary>
		public string OverwriteOnlyErrors()
		{
			return $@"
UPDATE
	[Resource].[{{0}}]
SET
	[Status] = [Status] + CASE
		WHEN [doc].[count_function] IS NULL OR [doc].[count_function] = 0 THEN {(long)Relativity.MassImport.DTO.ImportStatus.ErrorOverwrite}
		WHEN [doc].[count_function] >= 2 THEN {(long)Relativity.MassImport.DTO.ImportStatus.ErrorOverwriteMultipleKey}
															ELSE {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
	END
	FROM [Resource].[{{0}}] as rit
	LEFT OUTER JOIN (SELECT [d].[{{1}}],
		(COUNT(DISTINCT [d].ArtifactID)) AS [count_function]
									FROM [Document] [d]
									GROUP BY [d].[{{1}}]
									) AS [doc] ON rit.[DocumentIdentifier] = [doc].[{{1}}]
";
		}

		/// <summary>
		/// Format replace:
		/// ---------------
		/// 0: img temp table
		/// 1: identifier column name
		/// 2: artifact temp table
		/// 3: userID
		/// </summary>
		public string PopulateArtifactIdColumnOnTempTable()
		{
			return $@"
UPDATE
	[Resource].[{{0}}]
SET
	[{{0}}].ArtifactID = [Document].[ArtifactID],
	[{{0}}].IsNew = CASE
		WHEN EXISTS(SELECT [ArtifactID] FROM [Resource].[{{2}}] WHERE [{{2}}].[ArtifactID] = [{{0}}].[ArtifactID]) THEN 1
		ELSE 0
	END
FROM
	[Resource].[{{0}}]
INNER JOIN [Document] ON
	[Document].[{{1}}] = [{{0}}].DocumentIdentifier
WHERE
	[{{0}}].[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}

/* UpdateOverlayPermissions */";
		}

		public string RedactionOverwriteErrorsWithImprovedJoinOrder(string imageTempTableName, string identifierColumnName)
		{
			return $@"
UPDATE
	tmp
SET
	tmp.[Status] |= {(long)Relativity.MassImport.DTO.ImportStatus.ErrorRedaction}
FROM
	[Resource].[{imageTempTableName}] AS tmp
INNER JOIN [Document] AS d ON
	d.[{identifierColumnName}] = tmp.[DocumentIdentifier]
WHERE
	tmp.[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
	AND
	EXISTS(
		SELECT
			1
		FROM
			[File] AS f
		INNER JOIN [Redaction] AS r ON
			r.[FileGuid] = f.[Guid]
		WHERE
			f.[Type] = 1
			AND
			d.[ArtifactID] = f.[DocumentArtifactID]
	)
";
		}

		/// <summary>
		/// Parameters:
		/// -----------
		/// @userID
		/// 
		/// Format replace:
		/// ---------------
		/// 0: img temp table
		/// </summary>
		public string UpdateArtifactAuditRecords()
		{
			return $@"
UPDATE
	[Artifact]
SET
	[LastModifiedOn] = GETUTCDATE(),
	[LastModifiedBy] = @userID
FROM
	[Artifact]
INNER JOIN [Resource].[{{0}}] ON
	[Artifact].[ArtifactID] = [{{0}}].[ArtifactID]
WHERE
	[{{0}}].[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}";
		}

		/// <summary>
		/// Format replace:
		/// ---------------
		/// 0: img temp table
		/// 1: audit detail clause
		/// 2: Object Type (e.x. Document)
		/// 3: Key Field Column Name
		/// 4: FullText column definition
		/// </summary>
		public string UpdateAuditClause()
		{
			return $@"
;WITH ImportExtractedText AS (
	SELECT
		[DocumentIdentifier],
		[ExtractedTextEncodingPageCode],
		[IsNew]
		{{4}}
	FROM
		[Resource].[{{0}}]
	WHERE
		[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
		AND
		[IsNew] = 0
		AND
		ISNULL([ExtractedTextEncodingPageCode], '-1') <> '-1'
)
INSERT INTO AuditRecord(
	[ArtifactID],
	[Action],
	[Details],
	[TimeStamp],
	[UserID],
	[RequestOrigination],
	[RecordOrigination]
)
SELECT
	[{{2}}].ArtifactID,
	47,
	{{1}},
	GETUTCDATE(),
	@userID,
	@requestOrig,
	@recordOrig
FROM
	[ImportExtractedText]
INNER JOIN [{{2}}] ON
	[{{2}}].[{{3}}] COLLATE DATABASE_DEFAULT = [ImportExtractedText].[DocumentIdentifier] COLLATE DATABASE_DEFAULT
	/* FileFieldAuditJoin */
			";
		}

		/// <summary>
		/// Format replace:
		/// ---------------
		/// 0: img temp table
		/// </summary>
		public string UpdateDocumentImageCount()
		{
			return $@"
		WITH ImageCounts AS (
			SELECT
				ArtifactID,
				ImageCount = COUNT(Z.FileIdentifier)
			FROM
				[Resource].[{{0}}] Z
			WHERE
				Z.[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
			GROUP BY
				Z.ArtifactID
		)

		UPDATE
			[Document]
		SET
			[RelativityImageCount] = T.ImageCount
		FROM
			[Document]
		INNER JOIN [ImageCounts] T ON
			T.[ArtifactID] = Document.[ArtifactID]";
		}

		public string UpdateOverlayPermissions()
		{
			return $@"; WITH EditList(EditAcl) AS (
SELECT DISTINCT AccessControlListID
FROM AccessControlListPermission
WHERE
	PermissionID = 45
	AND
	EXISTS(
		SELECT GroupArtifactID FROM GroupUser WHERE GroupArtifactID = AccessControlListPermission.GroupID AND UserArtifactID = {{3}}
	)
)
UPDATE
	[Resource].[{{0}}]
SET
	[Status] = [Status] + {(long)Relativity.MassImport.DTO.ImportStatus.SecurityUpdate}
WHERE
	NOT [{{0}}].IsNew = 1
	AND
	[{{0}}].[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
	AND
	NOT EXISTS(SELECT AccessControlListID FROM [Artifact] INNER JOIN EditList ON EditList.EditAcl = [Artifact].[AccessControlListID] AND [Artifact].[ArtifactID] = [{{0}}].[ArtifactID])";
		}

		public string UpdateOverlayPermissionsForAppendOverlayMode()
		{
			return $@"; WITH EditList(EditAcl) AS (
SELECT DISTINCT AccessControlListID
FROM AccessControlListPermission
WHERE
	PermissionID = 45
	AND
	EXISTS(
		SELECT GroupArtifactID FROM GroupUser WHERE GroupArtifactID = AccessControlListPermission.GroupID AND UserArtifactID = @userID
	)
)
UPDATE
	[Resource].[{{0}}]
SET
	[Status] = [{{0}}].[Status] + {(long)Relativity.MassImport.DTO.ImportStatus.SecurityUpdate}
FROM
	[Resource].[{{0}}]
LEFT JOIN [Document]
	ON [Document].[{{2}}] = [{{0}}].[DocumentIdentifier]
WHERE
	NOT [Document].[ArtifactID] IS NULL
	AND
	[{{0}}].[Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
	AND
	NOT EXISTS(SELECT AccessControlListID FROM [Artifact] INNER JOIN EditList ON EditList.EditAcl = [Artifact].[AccessControlListID] AND [Artifact].[ArtifactID] = [Document].[ArtifactID])";
		}

		public string DoesColumnExistOnDocumentTable()
		{
			return @"
SELECT
    CASE WHEN EXISTS(
        SELECT 1 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE 
            TABLE_NAME = 'Document'
            AND
            COLUMN_NAME = @columnName
            AND
            TABLE_SCHEMA = 'EDDSDBO'
    ) THEN 1 
    ELSE 0 END";
		}
	}
}