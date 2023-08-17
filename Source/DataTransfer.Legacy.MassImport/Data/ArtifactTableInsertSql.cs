using System;
using Relativity.Data.MassImport;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.Data
{
	internal class ArtifactTableInsertSql : SqlQueryPart
	{
		public static ArtifactTableInsertSql WithDocument(
			TableNames tableNames, 
			string textIdentifierColumn,
			int fieldArtifactId)
		{
			return new ArtifactTableInsertSql(
				WithObjectPart(
					tableNames, 
					textIdentifierColumn, 
					textIdentifierColumn,
					fieldArtifactId, 
					(int) ArtifactType.Document, 
					string.Empty),
				tableNames, 
				fieldArtifactId);
		}

		public static ArtifactTableInsertSql WithObject(
			TableNames tableNames, 
			string textIdentifierColumn,
			string keyFieldIdentifierColumn,
			int fieldArtifactId, 
			int artifactTypeId, 
			string keyFieldCheck)
		{
			return new ArtifactTableInsertSql(
				WithObjectPart(
					tableNames, 
					textIdentifierColumn, 
					keyFieldIdentifierColumn,
					fieldArtifactId, 
					artifactTypeId, 
					keyFieldCheck), 
				tableNames, 
				fieldArtifactId);
		}

		public static ArtifactTableInsertSql WithAssociatedObjects(
			TableNames tableNames, 
			string textIdentifierColumn, 
			int fieldArtifactId, 
			int artifactTypeId, 
			int parentArtifactId, 
			int accessControlListId)
		{
			return new ArtifactTableInsertSql(
				WithAssociatedObjectsPart(
					tableNames, 
					textIdentifierColumn, 
					fieldArtifactId, 
					artifactTypeId, 
					parentArtifactId, 
					accessControlListId), 
				tableNames, 
				fieldArtifactId);
		}

		private readonly ISqlQueryPart withQuery;
		private readonly TableNames tableNames;
		private readonly int fieldArtifactId;

		private ArtifactTableInsertSql(ISqlQueryPart withQuery, TableNames tableNames, int fieldArtifactId)
		{
			this.withQuery = withQuery;
			this.tableNames = tableNames;
			this.fieldArtifactId = fieldArtifactId;
		}

		public override FormattableString SqlString
		{
			get
			{
				return $@"
{withQuery.ToString()}
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
			{fieldArtifactId}
		INTO [Resource].[{tableNames.Part}];

SELECT @@ROWCOUNT
";
			}
		}

		private static ISqlQueryPart WithObjectPart(
			TableNames tableNames, 
			string textIdentifierColumn, 
			string keyFieldIdentifierColumn,
			int fieldArtifactId, 
			int artifactTypeId, 
			string keyFieldCheck)
		{
			return new InlineSqlQuery($@"
;WITH IDSource AS (
	SELECT
		[ArtifactTypeID] = {artifactTypeId},
		[ParentArtifactID] = [kCura_Import_ParentFolderID],
		[AccessControlListID] = (SELECT [ParentAccessControlListID] FROM [Resource].[{tableNames.Parent}] WHERE [kCura_Import_ID] = N.[kCura_Import_ID]),
		[AccessControlListIsInherited] = 1,
		[CreatedOn] = @now,
		[LastModifiedOn] = @now,
		[CreatedBy] = @auditUserID,
		[LastModifiedBy] = @auditUserID,
		[TextIdentifier] = N.[{textIdentifierColumn}],
		[ContainerID] = @containerArtifactID,
		[Keywords] = '',
		[Notes] = '',
		[DeleteFlag] = 0,
		[kCura_Import_ID]
		FROM [Resource].[{tableNames.Native}] N
		WHERE
			N.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
			AND
			N.[{textIdentifierColumn}] IS NOT NULL {keyFieldCheck}
			AND
			NOT EXISTS
			(
				SELECT *
				FROM [Resource].[{tableNames.Part}] P
				JOIN [Resource].[{tableNames.Native}] N2
				ON P.kCura_Import_ID = N2.kCura_Import_ID
				WHERE N2.[{keyFieldIdentifierColumn}] = N.[{keyFieldIdentifierColumn}]
				AND P.[kCura_Import_IsNew] = 0
				AND P.FieldArtifactId = {fieldArtifactId}
			)
)
");
		}

		private static ISqlQueryPart WithAssociatedObjectsPart(TableNames tableNames, string textIdentifierColumn, int fieldArtifactId, int artifactTypeId, int parentArtifactId, int accessControlListId)
		{
			return new InlineSqlQuery($@"
;WITH IDSource AS (
	SELECT
		[ArtifactTypeID] = {artifactTypeId},
		[ParentArtifactID] = {parentArtifactId},
		[AccessControlListID] = {accessControlListId},
		[AccessControlListIsInherited] = 1,
		[CreatedOn] = @now,
		[LastModifiedOn] = @now,
		[CreatedBy] = @auditUserID,
		[LastModifiedBy] = @auditUserID,
		[TextIdentifier] = N.[{textIdentifierColumn}],
		[ContainerID] = @containerArtifactID,
		[Keywords] = '',
		[Notes] = '',
		[DeleteFlag] = 0,
		[kCura_Import_ID] = MIN([kCura_Import_ID])
		FROM [Resource].[{tableNames.Native}] N
		WHERE
			N.[kCura_Import_Status] = {(long)Relativity.MassImport.DTO.ImportStatus.Pending}
			AND
			N.[{textIdentifierColumn}] IS NOT NULL
			AND
			NOT EXISTS
			(
				SELECT *
				FROM [Resource].[{tableNames.Part}] P
				JOIN [Resource].[{tableNames.Native}] N2
				ON P.kCura_Import_ID = N2.kCura_Import_ID
				WHERE N2.[{textIdentifierColumn}] = N.[{textIdentifierColumn}]
				AND P.[kCura_Import_IsNew] = 0
				AND P.FieldArtifactId = {fieldArtifactId}
			)
		GROUP BY N.[{textIdentifierColumn}]
)
");
		}
	}
}