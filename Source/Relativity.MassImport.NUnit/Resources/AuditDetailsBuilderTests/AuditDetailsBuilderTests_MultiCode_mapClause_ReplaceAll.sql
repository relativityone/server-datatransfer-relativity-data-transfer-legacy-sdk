﻿,
		(
			SELECT
				MappedArtifactID [setChoice]
			FROM [Resource].[RELNATTMPMAP_AAD09DF6-A4C7-4DF0-B963-0050C7809000] M1
			WHERE M1.ArtifactID = M.ArtifactID AND M1.FieldArtifactID = 100123 AND M1.IsNew = 1
			FOR XML PATH (''), TYPE
		) [MultiCodeField IsNew],
		(
			SELECT
				MappedArtifactID [unsetChoice]
			FROM [Resource].[RELNATTMPMAP_AAD09DF6-A4C7-4DF0-B963-0050C7809000] M1
			WHERE M1.ArtifactID = M.ArtifactID AND M1.FieldArtifactID = 100123 AND M1.IsNew = 0
			FOR XML PATH (''), TYPE
		) [MultiCodeField]