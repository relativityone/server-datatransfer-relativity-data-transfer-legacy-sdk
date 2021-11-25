/*
	Format replace:
	---------------
	0: native tmp table
	1: code tmp table
	2: code type id
	3: identifier column name
	4: CodeArtifact partition table name
	5: field overlay switch statement
	6: staging table with audit information
	7: field artifactId
*/


IF EXISTS (
    SELECT CodeArtifactID FROM [Resource].[RELNATTMPCOD_974a2b26_d665_4f42_8b3b_31949b335a01]
    WHERE CodeTypeID = 101
    AND NOT EXISTS (SELECT 1 FROM [Code] WHERE [CodeTypeID] = 101 AND [ArtifactID] = [CodeArtifactID])
)
BEGIN
	RAISERROR('Some supplied choice ids are invalid',16,1)
END

IF 0 = 0
BEGIN
	DELETE FROM
		[ZCodeArtifact_101]
	OUTPUT DELETED.[AssociatedArtifactID], DELETED.[CodeArtifactID], 2, 0 INTO [Resource].[RELNATTMPMAP_974a2b26_d665_4f42_8b3b_31949b335a01]
	WHERE
		EXISTS(
			SELECT
				ArtifactID
			FROM
				[Resource].[RELNATTMP_974a2b26_d665_4f42_8b3b_31949b335a01]
			WHERE
				ArtifactID=[ZCodeArtifact_101].[AssociatedArtifactID]
				AND
				[RELNATTMP_974a2b26_d665_4f42_8b3b_31949b335a01].[kCura_Import_Status] = 0
		)

	INSERT INTO [ZCodeArtifact_101] (
		[AssociatedArtifactID],
		[CodeArtifactID]
	) OUTPUT INSERTED.[AssociatedArtifactID], INSERTED.[CodeArtifactID], 2, 1 INTO [Resource].[RELNATTMPMAP_974a2b26_d665_4f42_8b3b_31949b335a01]
	SELECT /* IncludeDisctinctClause */
		[RELNATTMP_974a2b26_d665_4f42_8b3b_31949b335a01].[ArtifactID],
		[RELNATTMPCOD_974a2b26_d665_4f42_8b3b_31949b335a01].[CodeArtifactID]
	FROM
		[Resource].[RELNATTMPCOD_974a2b26_d665_4f42_8b3b_31949b335a01]
	INNER JOIN [Resource].[RELNATTMP_974a2b26_d665_4f42_8b3b_31949b335a01] ON
		[RELNATTMP_974a2b26_d665_4f42_8b3b_31949b335a01].[Id] = [RELNATTMPCOD_974a2b26_d665_4f42_8b3b_31949b335a01].[DocumentIdentifier]
		AND
		[RELNATTMP_974a2b26_d665_4f42_8b3b_31949b335a01].[kCura_Import_Status] = 0
	WHERE
		[RELNATTMPCOD_974a2b26_d665_4f42_8b3b_31949b335a01].[CodeTypeID] = 101
END
ELSE
BEGIN
	INSERT INTO [ZCodeArtifact_101] (
		[AssociatedArtifactID],
		[CodeArtifactID]
	) OUTPUT INSERTED.[AssociatedArtifactID], INSERTED.[CodeArtifactID], 2, 1 INTO [Resource].[RELNATTMPMAP_974a2b26_d665_4f42_8b3b_31949b335a01]
	SELECT /* IncludeDisctinctClause */
		[RELNATTMP_974a2b26_d665_4f42_8b3b_31949b335a01].[ArtifactID],
		[RELNATTMPCOD_974a2b26_d665_4f42_8b3b_31949b335a01].[CodeArtifactID]
	FROM
		[Resource].[RELNATTMPCOD_974a2b26_d665_4f42_8b3b_31949b335a01]
	INNER JOIN [Resource].[RELNATTMP_974a2b26_d665_4f42_8b3b_31949b335a01] ON
		[RELNATTMP_974a2b26_d665_4f42_8b3b_31949b335a01].[Id] = [RELNATTMPCOD_974a2b26_d665_4f42_8b3b_31949b335a01].[DocumentIdentifier]
		AND
		[RELNATTMP_974a2b26_d665_4f42_8b3b_31949b335a01].[kCura_Import_Status] = 0
	LEFT JOIN [ZCodeArtifact_101] AS [ExistingChoices] ON
		[ExistingChoices].[AssociatedArtifactID] = [RELNATTMP_974a2b26_d665_4f42_8b3b_31949b335a01].[ArtifactID]
		AND
		[ExistingChoices].[CodeArtifactID] = [RELNATTMPCOD_974a2b26_d665_4f42_8b3b_31949b335a01].[CodeArtifactID]
	WHERE
		[RELNATTMPCOD_974a2b26_d665_4f42_8b3b_31949b335a01].[CodeTypeID] = 101
		AND
		[ExistingChoices].[AssociatedArtifactID] IS NULL
		AND
		[ExistingChoices].[CodeArtifactID] IS NULL
END