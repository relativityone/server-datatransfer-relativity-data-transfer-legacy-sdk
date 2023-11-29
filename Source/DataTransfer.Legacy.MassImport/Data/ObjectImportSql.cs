using Relativity.Data.MassImport;
using Relativity.MassImport.Data.SqlFramework;

namespace Relativity.MassImport.Data
{
	// Replaces .resx file
	internal class ObjectImportSql : NativeImportSql
	{
		public override string VerifyExistenceOfAssociatedMultiObjects(TableNames tableNames, string importedIdentifierColumn, string idFieldColumnName, string associatedObjectTable, FieldInfo field)
		{
			// *AK: "VerifyExistenceOfAssociatedMultiObjects" sql script was written for ImportAPI
			// and some references do not correspond to fields in temp tables used here
			// so we correct or/and remove those references below
			string associatedObjectSqlFormat = base.VerifyExistenceOfAssociatedMultiObjects(tableNames, importedIdentifierColumn, idFieldColumnName,  associatedObjectTable, field);
			return associatedObjectSqlFormat.Replace($"AND [{tableNames.Native}].[{field.GetColumnName()}] IS NOT NULL", "");
		}

		public override InlineSqlQuery PopulatePartTable(TableNames tableNames, string objectTable, int topFieldArtifactID, string keyField)
		{
			return new InlineSqlQuery($@"
INSERT INTO [Resource].[{tableNames.Part}]
SELECT
	N.[kCura_Import_ID],
	0 [kCura_Import_IsNew],
	A.[ArtifactID],
	A.[AccessControlListID],
	{topFieldArtifactID} [FieldArtifactID]
FROM [Resource].[{tableNames.Native}] N
JOIN [{objectTable}] O ON O.[{keyField}] = N.[{keyField}]
JOIN [EDDSDBO].[Artifact] A ON O.[ArtifactID] = A.[ArtifactID];
");
		}
	}
}