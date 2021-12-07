using Relativity.Data.MassImport;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.Data
{
	internal class MassImportSqlHelper
	{
		public static InlineSqlQuery CheckAddingObjectPermission(TableNames tableNames, int userID, int ArtifactTypeID)
		{
			return new InlineSqlQuery($@"
DECLARE @addArtifactPermissionId INT = (SELECT TOP 1 ArtifactTypePermission.PermissionID FROM ArtifactTypePermission INNER JOIN Permission ON Permission.PermissionID = ArtifactTypePermission.PermissionID And [Type] = 6 And ArtifactTypeID = {ArtifactTypeID})

UPDATE N
SET
	[kCura_Import_Status] = [kCura_Import_Status] + {(long)ImportStatus.SecurityAdd}
FROM [Resource].[{tableNames.Native}] N
LEFT JOIN [Resource].[{tableNames.Part}] P ON N.[kCura_Import_ID] = P.[kCura_Import_ID]
LEFT JOIN [Resource].[{tableNames.Parent}] P2 ON N.[kCura_Import_ID] = P2.[kCura_Import_ID]
WHERE
	P.[ArtifactID] is null
	AND	
	NOT EXISTS(
		SELECT
			AccessControlListID
		FROM
			AccessControlListPermission
		WHERE
			PermissionID = @addArtifactPermissionId
			AND
			AccessControlListID = P2.ParentAccessControlListID
			AND
			GroupID IN (SELECT GroupArtifactID FROM GroupUser WHERE UserArtifactID = {userID})
	)
");
		}

		public static InlineSqlQuery CheckParentIsFolder(TableNames tableNames)
		{
			return new InlineSqlQuery($@"
UPDATE N
SET
	[kCura_Import_Status] = [kCura_Import_Status] + {(long)ImportStatus.ErrorParentMustBeFolder}
FROM
	[Resource].[{tableNames.Native}] N
JOIN 
	[Resource].[{tableNames.Parent}] P2 
ON 
	N.[kCura_Import_ID] = P2.[kCura_Import_ID]
WHERE	
	P2.ParentArtifactTypeID != 9;
	");
		}
	}
}