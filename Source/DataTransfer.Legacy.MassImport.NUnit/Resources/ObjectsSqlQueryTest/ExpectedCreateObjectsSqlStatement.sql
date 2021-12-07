DECLARE @now DATETIME = GETUTCDATE()

;WITH IDSource AS (
	SELECT
		[ArtifactTypeID] = 1000050,
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

INSERT INTO [ObjectTableName] (
	[ArtifactID],
	[KeyFieldName],
	[SingleObjectFieldName]
) SELECT
	P.ArtifactID,
	N.[KeyFieldName],
	N.[SingleObjectFieldName]
FROM
	[Resource].[RELNATTMP_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N
INNER JOIN [Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P ON
	P.[kCura_Import_ID] = N.[kCura_Import_ID]
WHERE
	N.[kCura_Import_Status] = 0 AND P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = 0;

INSERT INTO ArtifactAncestry(
	ArtifactID,
	AncestorArtifactID
)	SELECT
		P.ArtifactID, AncestorArtifactID
	FROM
		[Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P
	JOIN [Resource].[RELNATTMPPARENT_DCD09DF6-A4C7-4DF0-B963-0050C7809038] PARENT ON P.[kCura_Import_ID] = PARENT.[kCura_Import_ID]
	INNER JOIN ArtifactAncestry ON ArtifactAncestry.ArtifactID = PARENT.ParentArtifactID
	WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = 0
	UNION ALL
	SELECT
		P.ArtifactID, AncestorArtifactID = PARENT.ParentArtifactID
	FROM
		[Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P
	JOIN [Resource].[RELNATTMPPARENT_DCD09DF6-A4C7-4DF0-B963-0050C7809038] PARENT ON P.[kCura_Import_ID] = PARENT.[kCura_Import_ID]
	WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = 0;

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
		'RequestOrigination',
		'RecordOrigination'
	FROM
		[Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P
	WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = 0;