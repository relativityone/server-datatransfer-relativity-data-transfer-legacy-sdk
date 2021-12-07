using Relativity.Data.MassImport;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.Data
{
	internal class ChoicesImportSql
	{
		private readonly bool _isAppendOnly;

		public TableNames TableNames { get; private set; }
		public string KeyColumnName { get; private set; }

		public ChoicesImportSql(TableNames tableNames, string keyColumnName, OverwriteType overwriteType)
		{
			_isAppendOnly = overwriteType == OverwriteType.Append;
			TableNames = tableNames;
			KeyColumnName = keyColumnName;
		}

		public ISqlQueryPart NotAllChoicesAreValidCondition()
		{
			return new InlineSqlQuery($@"EXISTS (
SELECT 
	T.CodeArtifactID 
FROM 
	[Resource].[{TableNames.Code}] AS T
LEFT OUTER JOIN 
	[Code] AS C
ON 
	T.CodeArtifactID = C.ArtifactID AND T.CodeTypeID = C.CodeTypeID
WHERE 
	C.ArtifactID IS NULL)");
		}

		public ISqlQueryPart RaiseInvalidChoicesError()
		{
			return new InlineSqlQuery($@"RAISERROR('Some supplied choice ids are invalid',16,1)");
		}

		public string ReplaceChoicesQuery(int codeTypeId, string codeMappingTable, int fieldArtifactID)
		{
			if (_isAppendOnly)
			{
				return InsertChoicesQuery(codeTypeId, codeMappingTable, fieldArtifactID);
			}
			else
			{
				return $@"
{DeleteChoicesQuery(codeMappingTable, fieldArtifactID)}
{InsertChoicesQuery(codeTypeId, codeMappingTable, fieldArtifactID, "")}";
			}
		}

		public string MergeMultiChoicesQuery(int codeTypeId, string codeMappingTable, int fieldArtifactID)
		{

			string whereClause = _isAppendOnly ? "" : $@"
WHERE 
	NOT EXISTS (
		SELECT		
			[ExistingChoices].[CodeArtifactID]
		FROM
			{codeMappingTable} [ExistingChoices] 
		WHERE
			[ExistingChoices].[AssociatedArtifactID] = CTE.[DocumentArtifactID] 
			AND [ExistingChoices].[CodeArtifactID] = CTE.[CodeArtifactID]
	)";

			return InsertChoicesQuery(codeTypeId, codeMappingTable, fieldArtifactID, whereClause);
		}

		private string DeleteChoicesQuery(string codeMappingTable, int fieldArtifactID)
		{
			return $@"
DELETE Z
OUTPUT 
	DELETED.[AssociatedArtifactID], 
	DELETED.[CodeArtifactID], 
	{fieldArtifactID}, 
	0 
INTO 
	[Resource].[{TableNames.Map}]
FROM 
	{codeMappingTable} Z
WHERE
	EXISTS(
		SELECT
			ArtifactID
		FROM
			[Resource].[{TableNames.Native}] N
		WHERE
			ArtifactID = Z.[AssociatedArtifactID]
			AND
			N.[kCura_Import_Status] = {(long)ImportStatus.Pending}
	);";
		}

		private string InsertChoicesQuery(int codeTypeId, string codeMappingTable, int fieldArtifactID, string whereClause = "")
		{
			return $@"
WITH CTE (DocumentArtifactID, CodeArtifactID)
AS
(
	SELECT /* IncludeDistinctClause */
		N.ArtifactID, C.CodeArtifactID
	FROM 
		[Resource].[{TableNames.Code}] C
	JOIN 
		[Resource].[{TableNames.Native}] N
	ON 
		N.[{KeyColumnName}] = C.[DocumentIdentifier]
	WHERE
		C.[CodeTypeID] = {codeTypeId}
		AND N.[kCura_Import_Status] = {(long)ImportStatus.Pending}
)

INSERT INTO {codeMappingTable} ([CodeArtifactID], [AssociatedArtifactID])
OUTPUT 
	INSERTED.[AssociatedArtifactID], 
	INSERTED.[CodeArtifactID], 
	{fieldArtifactID}, 
	1 
INTO 
	[Resource].[{TableNames.Map}]
SELECT 
	CodeArtifactID, 
	DocumentArtifactID 
FROM 
	CTE {whereClause};";
		}
	}
}