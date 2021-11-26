using System.Linq;
using kCura.Data.RowDataGateway;
using Relativity.Data;
using Relativity.Data.MassImport;
using Relativity.Data.Toggles;
using Relativity.Toggles;

namespace Relativity.MassImport.Data.Choices
{
	internal class OldChoicesImportService : IChoicesImportService
	{
		private readonly BaseContext _context;
		private readonly int _queryTimeoutInSeconds;
		private readonly IToggleProvider _toggleProvider;
		private readonly TableNames _tableNames;
		private readonly ImportMeasurements _importMeasurements;
		private readonly NativeLoadInfo _settings;

		public OldChoicesImportService(
			BaseContext context,
			IToggleProvider toggleProvider,
			TableNames tableNames,
			ImportMeasurements importMeasurements,
			NativeLoadInfo settings,
			int queryTimeoutInSeconds)
		{
			_context = context;
			_queryTimeoutInSeconds = queryTimeoutInSeconds;
			_toggleProvider = toggleProvider;
			_tableNames = tableNames;
			_importMeasurements = importMeasurements;
			_settings = settings;
		}

		public void PopulateCodeArtifactTable()
		{
			_importMeasurements.SecondaryArtifactCreationTime.Start();
			kCura.Utility.InjectionManager.Instance.Evaluate("44240a2c-196e-4e7d-822d-1bbd13c951a9");
			string sqlFormat = CodeArtifact();
			// 0: native tmp table
			// 1: code tmp table
			// 2: code type id
			// 3: identifier column name
			// 4: CodeArtifact partition table name
			// 5: field overlay switch statement
			// 6: staging table with audit information
			// 7: field artifactId

			string keyColumnName = GetKeyField().GetColumnName();
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach (FieldInfo mappedField in this._settings.MappedFields)
			{
				if (mappedField.Type == Relativity.FieldTypeHelper.FieldType.Code || mappedField.Type == Relativity.FieldTypeHelper.FieldType.MultiCode)
				{
					string codeArtifactTableName = CodeHelper.GetCodeArtifactTableNameByCodeTypeID(mappedField.CodeTypeID);
					string fieldOverlayExpression = Relativity.Data.MassImportOld.Helper.GetFieldOverlaySwitchStatement(this._settings, mappedField.Type, mappedField.ArtifactID.ToString());
					string queryChunk = string.Format(
						sqlFormat,
						_tableNames.Native, // 0
						_tableNames.Code, // 1
						mappedField.CodeTypeID, // 2
						keyColumnName, // 3
						codeArtifactTableName, // 4
						fieldOverlayExpression, // 5
						_tableNames.Map, // 6
						mappedField.ArtifactID); // 7

					if (mappedField.ImportBehavior == FieldInfo.ImportBehaviorChoice.ChoiceFieldIgnoreDuplicates &&
						_toggleProvider.IsEnabled<IgnoreDuplicateValuesForMassImportChoice>())
					{
						queryChunk = queryChunk.Replace("/* IncludeDisctinctClause */", "DISTINCT");
					}

					sb.Append(queryChunk);
				}
			}

			if (sb.ToString().Length > 0)
			{
				_context.ExecuteNonQuerySQLStatement(sb.ToString(), _queryTimeoutInSeconds);
			}

			kCura.Utility.InjectionManager.Instance.Evaluate("8e8bdb63-8adc-4db5-af8e-c1f54fe83ee0");
			_importMeasurements.SecondaryArtifactCreationTime.Stop();
		}

		private FieldInfo GetKeyField()
		{
			return _settings.MappedFields.FirstOrDefault(f => f.ArtifactID == _settings.KeyFieldArtifactID);
		}

		private string CodeArtifact()
		{
			return @"/*
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
    SELECT CodeArtifactID FROM [Resource].[{1}]
    WHERE CodeTypeID = {2}
    AND NOT EXISTS (SELECT 1 FROM [Code] WHERE [CodeTypeID] = {2} AND [ArtifactID] = [CodeArtifactID])
)
BEGIN
	RAISERROR('Some supplied choice ids are invalid',16,1)
END

IF {5} = 0
BEGIN
	DELETE FROM
		[{4}]
	OUTPUT DELETED.[AssociatedArtifactID], DELETED.[CodeArtifactID], {7}, 0 INTO [Resource].[{6}]
	WHERE
		EXISTS(
			SELECT
				ArtifactID
			FROM
				[Resource].[{0}]
			WHERE
				ArtifactID=[{4}].[AssociatedArtifactID]
				AND
				[{0}].[kCura_Import_Status] = 0
		)

	INSERT INTO [{4}] (
		[AssociatedArtifactID],
		[CodeArtifactID]
	) OUTPUT INSERTED.[AssociatedArtifactID], INSERTED.[CodeArtifactID], {7}, 1 INTO [Resource].[{6}]
	SELECT /* IncludeDisctinctClause */
		[{0}].[ArtifactID],
		[{1}].[CodeArtifactID]
	FROM
		[Resource].[{1}]
	INNER JOIN [Resource].[{0}] ON
		[{0}].[{3}] = [{1}].[DocumentIdentifier]
		AND
		[{0}].[kCura_Import_Status] = 0
	WHERE
		[{1}].[CodeTypeID] = {2}
END
ELSE
BEGIN
	INSERT INTO [{4}] (
		[AssociatedArtifactID],
		[CodeArtifactID]
	) OUTPUT INSERTED.[AssociatedArtifactID], INSERTED.[CodeArtifactID], {7}, 1 INTO [Resource].[{6}]
	SELECT /* IncludeDisctinctClause */
		[{0}].[ArtifactID],
		[{1}].[CodeArtifactID]
	FROM
		[Resource].[{1}]
	INNER JOIN [Resource].[{0}] ON
		[{0}].[{3}] = [{1}].[DocumentIdentifier]
		AND
		[{0}].[kCura_Import_Status] = 0
	LEFT JOIN [{4}] AS [ExistingChoices] ON
		[ExistingChoices].[AssociatedArtifactID] = [{0}].[ArtifactID]
		AND
		[ExistingChoices].[CodeArtifactID] = [{1}].[CodeArtifactID]
	WHERE
		[{1}].[CodeTypeID] = {2}
		AND
		[ExistingChoices].[AssociatedArtifactID] IS NULL
		AND
		[ExistingChoices].[CodeArtifactID] IS NULL
END";
		}
	}
}