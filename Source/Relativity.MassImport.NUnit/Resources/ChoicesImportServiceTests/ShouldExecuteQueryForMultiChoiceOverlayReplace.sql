IF EXISTS (
SELECT 
	T.CodeArtifactID 
FROM 
	[Resource].[RELNATTMPCOD_974a2b26_d665_4f42_8b3b_31949b335a01] AS T
LEFT OUTER JOIN 
	[Code] AS C
ON 
	T.CodeArtifactID = C.ArtifactID AND T.CodeTypeID = C.CodeTypeID
WHERE 
	C.ArtifactID IS NULL)
BEGIN
RAISERROR('Some supplied choice ids are invalid',16,1)
END
ELSE
BEGIN
SET STATISTICS TIME ON;


PRINT 'Mass Import Section: LinkCodeTypeId=102';


DELETE Z
OUTPUT 
	DELETED.[AssociatedArtifactID], 
	DELETED.[CodeArtifactID], 
	2, 
	0 
INTO 
	[Resource].[RELNATTMPMAP_974a2b26_d665_4f42_8b3b_31949b335a01]
FROM 
	ZCodeArtifact_102 Z
WHERE
	EXISTS(
		SELECT
			ArtifactID
		FROM
			[Resource].[RELNATTMP_974a2b26_d665_4f42_8b3b_31949b335a01] N
		WHERE
			ArtifactID = Z.[AssociatedArtifactID]
			AND
			N.[kCura_Import_Status] = 0
	);

WITH CTE (DocumentArtifactID, CodeArtifactID)
AS
(
	SELECT /* IncludeDistinctClause */
		N.ArtifactID, C.CodeArtifactID
	FROM 
		[Resource].[RELNATTMPCOD_974a2b26_d665_4f42_8b3b_31949b335a01] C
	JOIN 
		[Resource].[RELNATTMP_974a2b26_d665_4f42_8b3b_31949b335a01] N
	ON 
		N.[Id] = C.[DocumentIdentifier]
	WHERE
		C.[CodeTypeID] = 102
		AND N.[kCura_Import_Status] = 0
)

INSERT INTO ZCodeArtifact_102 ([CodeArtifactID], [AssociatedArtifactID])
OUTPUT 
	INSERTED.[AssociatedArtifactID], 
	INSERTED.[CodeArtifactID], 
	2, 
	1 
INTO 
	[Resource].[RELNATTMPMAP_974a2b26_d665_4f42_8b3b_31949b335a01]
SELECT 
	CodeArtifactID, 
	DocumentArtifactID 
FROM 
	CTE ;
SET STATISTICS TIME OFF;

END