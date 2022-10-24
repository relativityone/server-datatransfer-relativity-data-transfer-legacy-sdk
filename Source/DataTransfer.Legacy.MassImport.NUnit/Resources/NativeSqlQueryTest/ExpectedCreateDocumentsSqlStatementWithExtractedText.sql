DECLARE @now DATETIME = GETUTCDATE()DECLARE @hasImagesCodeTypeID INT = (SELECT TOP 1 CodeTypeID FROM CodeType WHERE [Name] = 'HasImages')DECLARE @hasImagesCodeArtifactID INT = (SELECT TOP 1 [ArtifactID] FROM [Code] WHERE [Name]='No' AND CodeTypeID = @hasImagesCodeTypeID )

;WITH IDSource AS (
	SELECT
		[ArtifactTypeID] = 10,
		[ParentArtifactID] = [kCura_Import_ParentFolderID],
		[AccessControlListID] = (SELECT [ParentAccessControlListID] FROM [Resource].[RELNATTMPPARENT_DCD09DF6-A4C7-4DF0-B963-0050C7809038] WHERE [kCura_Import_ID] = N.[kCura_Import_ID]),
		[AccessControlListIsInherited] = 1,
		[CreatedOn] = @now,
		[LastModifiedOn] = @now,
		[CreatedBy] = @auditUserID,
		[LastModifiedBy] = @auditUserID,
		[TextIdentifier] = N.[KeyFieldName],
		[ContainerID] = @containerArtifactID,
		[Keywords] = '',
		[Notes] = '',
		[DeleteFlag] = 0,
		[kCura_Import_ID]
		FROM [Resource].[RELNATTMP_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N
		WHERE
			N.[kCura_Import_Status] = 0
			AND
			N.[KeyFieldName] IS NOT NULL 
			AND
			NOT EXISTS
			(
				SELECT *
				FROM [Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P
				JOIN [Resource].[RELNATTMP_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N2
				ON P.kCura_Import_ID = N2.kCura_Import_ID
				WHERE N2.[KeyFieldName] = N.[KeyFieldName]
				AND P.[kCura_Import_IsNew] = 0
				AND P.FieldArtifactId = 0
			)
)

MERGE INTO Artifact USING IDSource ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (
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
    VALUES (
			IDSource.[ArtifactTypeID],
			IDSource.[ParentArtifactID],
			IDSource.[AccessControlListID],
			IDSource.[AccessControlListIsInherited],
			IDSource.[CreatedOn],
			IDSource.[LastModifiedOn],
			IDSource.[CreatedBy],
			IDSource.[LastModifiedBy],
			IDSource.[TextIdentifier],
			IDSource.[ContainerID],
			IDSource.[Keywords],
			IDSource.[Notes],
			IDSource.[DeleteFlag]
		)
    OUTPUT
			IDSource.[kCura_Import_ID],
			1,
			INSERTED.ArtifactID,
			NULL,
			0
		INTO [Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038];

SELECT @@ROWCOUNT

INSERT INTO [Document] (
	[ArtifactID],
	[AccessControlListID_D],
	[ParentArtifactID_D],
	[KeyFieldName],
	[SingleObjectFieldName]
) SELECT
	P.[ArtifactID],
	PARENT.[ParentAccessControlListID],
	PARENT.[ParentArtifactID],
	N.[KeyFieldName],
	N.[SingleObjectFieldName]
FROM
	[Resource].[RELNATTMP_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N
INNER JOIN [Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P ON P.[kCura_Import_ID] = N.[kCura_Import_ID]
INNER JOIN [Resource].[RELNATTMPPARENT_DCD09DF6-A4C7-4DF0-B963-0050C7809038] PARENT ON PARENT.[kCura_Import_ID] = N.[kCura_Import_ID]
WHERE
	N.[kCura_Import_Status] = 0 AND P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = 0;

DROP TABLE IF EXISTS [#RELNATPARENTANCESTORS_DCD09DF6-A4C7-4DF0-B963-0050C7809038], [#RELNATNEWANCESTORS_DCD09DF6-A4C7-4DF0-B963-0050C7809038];

SELECT P.ArtifactID, PARENT.ParentArtifactID
INTO [#RELNATNEWANCESTORS_DCD09DF6-A4C7-4DF0-B963-0050C7809038]
FROM [Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P
JOIN [Resource].[RELNATTMPPARENT_DCD09DF6-A4C7-4DF0-B963-0050C7809038] PARENT ON P.[kCura_Import_ID] = PARENT.[kCura_Import_ID]
WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = 0;

ALTER TABLE [#RELNATNEWANCESTORS_DCD09DF6-A4C7-4DF0-B963-0050C7809038] ADD PRIMARY KEY(ArtifactID);

SELECT N.ArtifactID, A.AncestorArtifactID
INTO [#RELNATPARENTANCESTORS_DCD09DF6-A4C7-4DF0-B963-0050C7809038]
FROM [#RELNATNEWANCESTORS_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N
JOIN [EDDSDBO].ArtifactAncestry A ON A.ArtifactID = N.ParentArtifactID;

ALTER TABLE [#RELNATPARENTANCESTORS_DCD09DF6-A4C7-4DF0-B963-0050C7809038] ADD PRIMARY KEY(ArtifactID,AncestorArtifactID);

INSERT INTO [EDDSDBO].ArtifactAncestry(ArtifactID, AncestorArtifactID)
SELECT ArtifactID, ParentArtifactID FROM [#RELNATNEWANCESTORS_DCD09DF6-A4C7-4DF0-B963-0050C7809038]
UNION ALL
SELECT ArtifactID, AncestorArtifactID FROM [#RELNATPARENTANCESTORS_DCD09DF6-A4C7-4DF0-B963-0050C7809038];

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
		
[ExtractedTextEncodingPageCode] =
	 CASE
			WHEN [ExtractedTextEncodingPageCode] IS NULL THEN '<auditElement><extractedTextEncodingPageCode>' + '-1' + '</extractedTextEncodingPageCode></auditElement>'
			else '<auditElement><extractedTextEncodingPageCode>' +  CAST([ExtractedTextEncodingPageCode] as varchar(200)) + '</extractedTextEncodingPageCode></auditElement>'
	 END,
		@userID As [UserID],
		GETUTCDATE() As [TimeStamp],
		'RecordOrigination' AS [RequestOrigination],
		'RecordOrigination' AS [RecordOrigination]
FROM
	[Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P
	/* Do this inner clause only if extracted text is enabled */
	INNER JOIN [Resource].[RELNATTMP_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N ON
	N.[kCura_Import_ID] = P.[kCura_Import_ID]
WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = 0;

INSERT INTO [DataGridRecordMapping] (
	[DocumentArtifactID]
) SELECT
		[ArtifactID]
	FROM
		[Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P
	WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = 0;

/*
Parameters:
	@hasImagesCodeArtifactID
*/
INSERT INTO [CodeArtifactTableName] (
	[CodeArtifactID],
	[AssociatedArtifactID]
)	SELECT
		@hasImagesCodeArtifactID,
		ArtifactID
	FROM
		[Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P
	WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = 0