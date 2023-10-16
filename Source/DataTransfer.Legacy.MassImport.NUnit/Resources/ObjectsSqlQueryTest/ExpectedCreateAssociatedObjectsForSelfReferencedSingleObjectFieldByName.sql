DECLARE @now DATETIME = GETUTCDATE()
INSERT INTO [Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038]
SELECT
	N.[kCura_Import_ID],
	0 [kCura_Import_IsNew],
	D.[ArtifactID],
	NULL,
	100123 [FieldArtifactID]
FROM (
	SELECT
		MIN(N.[kCura_Import_ID]) [kCura_Import_ID],
		N.[SingleObjectFieldName]
	FROM [Resource].[RELNATTMP_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N
	GROUP BY N.[SingleObjectFieldName]
) N
JOIN [SingleObjectTableName] D ON D.[SingleObjectIdFieldColumnName] = N.[SingleObjectFieldName];

;WITH DuplicatedAssociatedObjects(kCura_Import_ID) AS
(
	SELECT P.[kCura_Import_ID]
	FROM [Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P
	WHERE P.[FieldArtifactID] = 100123
	GROUP BY P.[kCura_Import_ID]
	HAVING COUNT(P.[kCura_Import_ID]) > 1
)

UPDATE N2
SET
	N2.[kCura_Import_Status] = N2.[kCura_Import_Status] + 262144,
    N2.[kCura_Import_ErrorData] = N. [SingleObjectFieldName] + '|SingleObjectTableName' + '|@fieldDisplayName'
FROM
	[Resource].[RELNATTMP_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N
JOIN [DuplicatedAssociatedObjects] ON
	N.[kCura_Import_ID] = [DuplicatedAssociatedObjects].[kCura_Import_ID]
JOIN [Resource].[RELNATTMP_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N2 ON
	N.[SingleObjectFieldName] = N2.[SingleObjectFieldName]
WHERE N2.[kCura_Import_Status] & 262144 = 0;
DECLARE @objectTypeIsChild INT;
SET @objectTypeIsChild =
CASE
	WHEN EXISTS
		(SELECT * FROM [ObjectType]
		WHERE
			[ObjectType].[DescriptorArtifactTypeID] = 1000050
			AND NOT [ObjectType].ParentArtifactTypeID = 8
			AND NOT [ObjectType].ParentArtifactTypeID = 9
		)
	THEN 1
	ELSE 0
END;

UPDATE N
SET
	[kCura_Import_Status] = [kCura_Import_Status] + 1048576,
    [kCura_Import_ErrorData] = @fieldDisplayName
FROM [Resource].[RELNATTMP_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N
WHERE
	@objectTypeIsChild = 1
	AND N.[SingleObjectFieldName] IS NOT NULL
	AND NOT EXISTS
	(
		SELECT 1
		FROM [Resource].[RELNATTMP_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N2
		JOIN [Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P
		ON P.[kCura_Import_ID] = N2.[kCura_Import_ID]
		WHERE
			P.[FieldArtifactID] = 100123
			AND N2.[SingleObjectFieldName] = N.[SingleObjectFieldName]
	)	
	AND N.[kCura_Import_Status] & 1048576 = 0;

;WITH IDSource AS (
	SELECT
		[ArtifactTypeID] = 1000050,
		[ParentArtifactID] = 11,
		[AccessControlListID] = 21,
		[AccessControlListIsInherited] = 1,
		[CreatedOn] = @now,
		[LastModifiedOn] = @now,
		[CreatedBy] = @auditUserID,
		[LastModifiedBy] = @auditUserID,
		[TextIdentifier] = N.[SingleObjectFieldName],
		[ContainerID] = @containerArtifactID,
		[Keywords] = '',
		[Notes] = '',
		[DeleteFlag] = 0,
		[kCura_Import_ID] = MIN([kCura_Import_ID])
		FROM [Resource].[RELNATTMP_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N
		WHERE
			N.[kCura_Import_Status] = 0
			AND
			N.[SingleObjectFieldName] IS NOT NULL
			AND
			NOT EXISTS
			(
				SELECT *
				FROM [Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P
				JOIN [Resource].[RELNATTMP_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N2
				ON P.kCura_Import_ID = N2.kCura_Import_ID
				WHERE N2.[SingleObjectFieldName] = N.[SingleObjectFieldName]
				AND P.[kCura_Import_IsNew] = 0
				AND P.FieldArtifactId = 100123
			)
		GROUP BY N.[SingleObjectFieldName]
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
			100123
		INTO [Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038];

SELECT @@ROWCOUNT

INSERT INTO [Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038]
SELECT
		N2.[kCura_Import_Id],
		0 [kCura_Import_IsNew],
		P.[ArtifactID],
		21,
		0 [FieldArtifactID]
FROM [Resource].[RELNATTMP_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N 
		JOIN [Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P 
		ON N.[kCura_Import_ID] = P.[kCura_Import_ID] 
		JOIN [Resource].[RELNATTMP_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N2
		ON N.[SingleObjectFieldName] = N2.[SingleObjectIdFieldColumnName]
WHERE P.[FieldArtifactID] = 100123
		AND P.[kCura_Import_IsNew] = 1

INSERT INTO [SingleObjectTableName] (
	[ArtifactID],
	[SingleObjectIdFieldColumnName]
)
SELECT DISTINCT
	P.ArtifactID,
	N.[SingleObjectFieldName]
FROM
	[Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P
INNER JOIN [Resource].[RELNATTMP_DCD09DF6-A4C7-4DF0-B963-0050C7809038] N ON
	N.[kCura_Import_ID] = P.[kCura_Import_ID] AND N.[kCura_Import_Status] = 0
WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = 100123;

INSERT INTO ArtifactAncestry(
	ArtifactID,
	AncestorArtifactID
)	SELECT
		P.ArtifactID, 11 AncestorArtifactID
	FROM
		[Resource].[RELNATTMPPART_DCD09DF6-A4C7-4DF0-B963-0050C7809038] P
	WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = 100123;

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
	WHERE P.[kCura_Import_IsNew] = 1 AND P.[FieldArtifactID] = 100123;