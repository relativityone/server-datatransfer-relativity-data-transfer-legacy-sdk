-- CACHE PREPARATION SECTION

										-- Check for table existence
											-- if it exists, check that it's not stale (has rows, but all are older than specified min)
												-- if it's stale data, delete out the rows
										-- if it doesn't exist, create the table
										-- NOTE: Purposely not using the CURRENT_TIMESTAMP for the date column, as it requires row update, and you may not have one

											IF OBJECT_ID('[Resource].[NAT_FOLDER_100_CONFIG]','TABLE') IS NULL
												BEGIN
													CREATE TABLE [Resource].[NAT_FOLDER_100_CONFIG](
														[CacheDurationMinutes] [INT] NOT NULL,
														[LastUpdated] [DATETIME] DEFAULT GETUTCDATE()
													)
													DECLARE @defaultExpired DATETIME = (DATEADD(MINUTE, -20, GETUTCDATE()))
													INSERT INTO [Resource].[NAT_FOLDER_100_CONFIG] VALUES(15, @defaultExpired)
												END

											IF OBJECT_ID('[Resource].[NAT_FOLDER_100]', 'TABLE') IS NOT NULL
												BEGIN
													DECLARE @cacheDurationMinutes INT = (SELECT TOP 1 [CacheDurationMinutes] FROM [Resource].[NAT_FOLDER_100_CONFIG]);
													DECLARE @negativeCacheDuration INT = (-1 * @cacheDurationMinutes);
													DECLARE @minDate DATETIME = (DATEADD(MINUTE, @negativeCacheDuration, GETUTCDATE()))
													IF (SELECT TOP 1 [LastUpdated] FROM [Resource].[NAT_FOLDER_100_CONFIG]) < @minDate
													BEGIN
														TRUNCATE TABLE[Resource].[NAT_FOLDER_100]
		END
											   END
											ELSE
												BEGIN
													CREATE TABLE[Resource].[NAT_FOLDER_100]
		(

													   [ID][INT] IDENTITY(1,1) NOT NULL,

													   [ArtifactID] [INT]
		NOT NULL,

													   [FullPath] [NVARCHAR] (max) NOT NULL,
														[PathSHA1] [BINARY] (20) NULL
													)
													CREATE INDEX IX_Resource_NAT_FOLDER_100_ArtifactID ON[Resource].[NAT_FOLDER_100]
		([ArtifactID])
											   END

										-- make a table variable that's a join of [Folder] and the [Cache]
											IF OBJECT_ID('tempdb..#ArtifactsJoined_100') IS NULL
												BEGIN
													CREATE TABLE #ArtifactsJoined_100
													(
														[FolderArtifactID] int NULL,
														[CacheArtifactID] int NULL
													)
													CREATE INDEX IX_#ArtifactsJoined_100_CacheArtifactID ON #ArtifactsJoined_100 ([CacheArtifactID]);
												END
											ELSE
												BEGIN
													TRUNCATE TABLE #ArtifactsJoined_100
												END

											INSERT INTO  #ArtifactsJoined_100 SELECT f.[ArtifactID], c.[ArtifactID]
												FROM[EDDS100].[EDDSDBO].[Folder]
		AS f
												FULL OUTER JOIN[Resource].[NAT_FOLDER_100]
		AS c
												ON f.[ArtifactID] = c.[ArtifactID]

										-- remove items which have been deleted from folders (FolderArtifactID is null)
											DELETE FROM[Resource].[NAT_FOLDER_100]
		WHERE ArtifactID IN(SELECT[CacheArtifactID] FROM  #ArtifactsJoined_100 WHERE [FolderArtifactID] is NULL)

										-- the following IF short-circuits away from the fullpath calculation, which would otherwise be done ahead of the where clause. And the SHA update only needs to be done if there has been an insert
		IF EXISTS(SELECT TOP 1 * FROM  #ArtifactsJoined_100 WHERE [CacheArtifactID] IS NULL)
											BEGIN
											-- items in the temp table where there's no CacheArtifactID must be added to the cache
											-- NOTE: FullPath is lowercase here

			INSERT INTO [Resource].[NAT_FOLDER_100] ([ArtifactID], [FullPath])
				SELECT

							  F.ArtifactID,
							  COALESCE(STUFF((
									SELECT '\' + ParentFolder.[Name]

									FROM[EDDS100].[EDDSDBO].[ArtifactAncestry] AS FolderAncestry

									INNER JOIN [EDDS100].[EDDSDBO].[Folder] ParentFolder WITH(NOLOCK) ON ParentFolder.[ArtifactID] = FolderAncestry.[AncestorArtifactID]

									WHERE F.[ArtifactID] = FolderAncestry.[ArtifactID]

									ORDER BY(SELECT COUNT(*) FROM [EDDS100].[EDDSDBO].[ArtifactAncestry] AS Depth WHERE FolderAncestry.[AncestorArtifactID] = Depth.[ArtifactID]) ASC, ParentFolder.[Name] ASC
																		FOR XML PATH('')), 1, 1, '') + '\' + (SELECT F.[Name] AS "data()" FOR XML PATH ('')), (SELECT F.Name AS "data()" FOR XML PATH(''))) [FullPath]
															FROM
																  [EDDS100].[EDDSDBO].[Folder] AS F
															INNER JOIN
																  [EDDS100].[EDDSDBO].[ExtendedArtifact] EA
																ON
																  F.[ArtifactID] = EA.[ArtifactID]
															LEFT JOIN
																  [EDDS100].[EDDSDBO].[Folder]
			AS ParentFolder
																  ON
																  ParentFolder.[ArtifactID] = EA.[ParentArtifactID]
													INNER JOIN #ArtifactsJoined_100 AS AJ ON F.[ArtifactID] = AJ.[FolderArtifactID] WHERE AJ.[CacheArtifactID] is NULL

											-- generate SHA for any rows where it's null with full path length less than or equal to 4k (because nvarchar4k = 8k bytes)
												UPDATE[Resource].[NAT_FOLDER_100] SET[PathSHA1] = HASHBYTES('SHA1', LOWER([FullPath]))
																WHERE[PathSHA1] Is NULL And DATALENGTH([FullPath]) <= 4000
											END

										-- UPDATE Cache LastModified
											DECLARE @updatedTime DATETIME = GETUTCDATE()
											UPDATE[Resource].[NAT_FOLDER_100_CONFIG] SET[LastUpdated] = @updatedTime

				-- bulk table manipulation time!
											DECLARE @shortenedRootFolderPath NVARCHAR(MAX) = (SELECT FCOAL.[FullPath]
		FROM(SELECT
																  F.ArtifactID,
																  COALESCE(STUFF((
																		SELECT '\' + ParentFolder.[Name]
																		FROM[EDDS100].[EDDSDBO].[ArtifactAncestry] AS FolderAncestry
																		INNER JOIN [EDDS100].[EDDSDBO].[Folder] ParentFolder WITH(NOLOCK) ON ParentFolder.[ArtifactID] = FolderAncestry.[AncestorArtifactID]
																		WHERE F.[ArtifactID] = FolderAncestry.[ArtifactID]
																		ORDER BY(SELECT COUNT(*) FROM [EDDS100].[EDDSDBO].[ArtifactAncestry] AS Depth WHERE FolderAncestry.[AncestorArtifactID] = Depth.[ArtifactID]) ASC, ParentFolder.[Name] ASC
																		FOR XML PATH('')), 1, 1, '') + '\' + (SELECT F.[Name] AS "data()" FOR XML PATH ('')), (SELECT F.Name AS "data()" FOR XML PATH(''))) [FullPath]
															FROM
																  [EDDS100].[EDDSDBO].[Folder]
		AS F
															INNER JOIN
																  [EDDS100].[EDDSDBO].[ExtendedArtifact]
		EA
ON
																  F.[ArtifactID] = EA.[ArtifactID]
															LEFT JOIN
																  [EDDS100].[EDDSDBO].[Folder]
		AS ParentFolder
																  ON
																  ParentFolder.[ArtifactID] = EA.[ParentArtifactID] WHERE F.[ArtifactID] = 100) AS FCOAL)
											DECLARE @shortenedRootFolderPathNonXML NVARCHAR(MAX) = (SELECT FCOAL.[FullPath]
		FROM(SELECT
																  F.ArtifactID,
																  COALESCE(STUFF((
																		SELECT '\' + ParentFolder.[Name]
																		FROM[EDDS100].[EDDSDBO].[ArtifactAncestry] AS FolderAncestry
																		INNER JOIN [EDDS100].[EDDSDBO].[Folder] ParentFolder WITH(NOLOCK) ON ParentFolder.[ArtifactID] = FolderAncestry.[AncestorArtifactID]
																		WHERE F.[ArtifactID] = FolderAncestry.[ArtifactID]
																		ORDER BY(SELECT COUNT(*) FROM [EDDS100].[EDDSDBO].[ArtifactAncestry] AS Depth WHERE FolderAncestry.[AncestorArtifactID] = Depth.[ArtifactID]) ASC, ParentFolder.[Name] ASC
																		FOR XML PATH(''),TYPE).value('.','NVARCHAR(MAX)'), 1, 1, '') + '\' + F.[Name], F.[Name]) [FullPath]
															FROM
																  [EDDS100].[EDDSDBO].[Folder]
		AS F
															INNER JOIN
																  [EDDS100].[EDDSDBO].[ExtendedArtifact]
		EA
ON
																  F.[ArtifactID] = EA.[ArtifactID]
															LEFT JOIN
																  [EDDS100].[EDDSDBO].[Folder]
		AS ParentFolder
																  ON
																  ParentFolder.[ArtifactID] = EA.[ParentArtifactID] WHERE F.[ArtifactID] = 100) AS FCOAL)
											DECLARE @maxBulkFolderPathLength INT = 4000 - DATALENGTH(@shortenedRootFolderPath)
											DECLARE @useHashForJoin INT = 0

										-- UPDATE the bulk table with a join against the cache.Join on hash if possible (4k), full path otherwise.
										   UPDATE[EDDS100].[Resource].[RELNATTMP_76a13eaa-430c-4c9a-9009-4fd046114fe6] SET[kCura_Import_ParentFolderPathXMLEncoded] = (@shortenedRootFolderPath + (SELECT[kCura_Import_ParentFolderPath] AS "data()" FOR XML PATH('')))
											UPDATE[EDDS100].[Resource].[RELNATTMP_76a13eaa-430c-4c9a-9009-4fd046114fe6] SET[kCura_Import_ParentFolderPath] = (@shortenedRootFolderPathNonXML + [kCura_Import_ParentFolderPath])

											IF(@maxBulkFolderPathLength <  (SELECT MAX(DATALENGTH([kCura_Import_ParentFolderPathXMLEncoded])) FROM[EDDS100].[Resource].[RELNATTMP_76a13eaa-430c-4c9a-9009-4fd046114fe6]))
											BEGIN
												-- Update with a JOIN on FullPath

												UPDATE[EDDS100].[Resource].[RELNATTMP_76a13eaa-430c-4c9a-9009-4fd046114fe6] SET[kCura_Import_ParentFolderID] = C.[ArtifactID]
FROM[EDDS100].[Resource].[RELNATTMP_76a13eaa-430c-4c9a-9009-4fd046114fe6] B INNER JOIN[Resource].[NAT_FOLDER_100]
		C
ON B.[kCura_Import_ParentFolderPathXMLEncoded] = C.[FullPath]
END
											ELSE
											BEGIN
												-- Perform the Update with a JOIN on Hash

												UPDATE[EDDS100].[Resource].[RELNATTMP_76a13eaa-430c-4c9a-9009-4fd046114fe6] SET[kCura_Import_ParentFolderID] = C.[ArtifactID]
FROM[EDDS100].[Resource].[RELNATTMP_76a13eaa-430c-4c9a-9009-4fd046114fe6] B INNER JOIN[Resource].[NAT_FOLDER_100]
		C
ON(CAST(HASHBYTES('SHA1', LOWER(B.[kCura_Import_ParentFolderPathXMLEncoded])) AS BINARY(20))) = C.[PathSHA1]
END

										-- return rows with negative folder id as folders which need to be created
											SELECT DISTINCT[kCura_Import_ParentFolderPath], [kCura_Import_ParentFolderPathXMLEncoded] FROM[EDDS100].[Resource].[RELNATTMP_76a13eaa-430c-4c9a-9009-4fd046114fe6] WHERE[kCura_Import_ParentFolderID] < 0
													 