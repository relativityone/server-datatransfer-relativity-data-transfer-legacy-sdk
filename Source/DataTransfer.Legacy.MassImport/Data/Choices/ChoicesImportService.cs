using System.Collections.Generic;
using System.Linq;
using kCura.Data.RowDataGateway;
using kCura.Utility;
using Relativity.Data.MassImport;
using Relativity.Data.Toggles;
using Relativity.MassImport.Data.Cache;
using Relativity.MassImport.Data.SqlFramework;
using Relativity.MassImport.Extensions;
using Relativity.Toggles;

namespace Relativity.MassImport.Data.Choices
{
	internal class ChoicesImportService : IChoicesImportService
	{
		private readonly BaseContext _context;
		private readonly int _queryTimeoutInSeconds;
		private readonly ImportMeasurements _importMeasurements;
		private readonly IToggleProvider _toggleProvider;
		private readonly TableNames _tableNames;
		private readonly IColumnDefinitionCache _columnDefinitionCache;
		private readonly Relativity.MassImport.DTO.NativeLoadInfo _settings;

		public ChoicesImportService(
			BaseContext context,
			IToggleProvider toggleProvider,
			TableNames tableNames,
			ImportMeasurements importMeasurements,
			Relativity.MassImport.DTO.NativeLoadInfo settings,
			IColumnDefinitionCache columnDefinitionCache,
			int queryTimeoutInSeconds)
		{
			_context = context;
			_queryTimeoutInSeconds = queryTimeoutInSeconds;
			_toggleProvider = toggleProvider;
			_tableNames = tableNames;
			_importMeasurements = importMeasurements;
			_settings = settings;
			_columnDefinitionCache = columnDefinitionCache;
		}

		public void PopulateCodeArtifactTable()
		{
			_importMeasurements.StartMeasure();
			_importMeasurements.SecondaryArtifactCreationTime.Start();
			InjectionManager.Instance.Evaluate("44240a2c-196e-4e7d-822d-1bbd13c951a9");
			string keyColumnName = _settings.GetKeyField().GetColumnName();
			var choicesImportSql = new ChoicesImportSql(_tableNames, keyColumnName, _settings.Overlay);
			var queryChunks = new List<ISqlQueryPart>();
			var codeAndMultiCodeFields = _settings.MappedFields
				.Where(info => info.Type == FieldTypeHelper.FieldType.Code || info.Type == FieldTypeHelper.FieldType.MultiCode);
			foreach (FieldInfo mappedField in codeAndMultiCodeFields)
			{
				string codeArtifactTableName = Relativity.Data.CodeHelper.GetCodeArtifactTableNameByCodeTypeID(mappedField.CodeTypeID);
				string queryChunk;
				if (Helper.IsMergeOverlayBehavior(_settings.OverlayBehavior, mappedField.Type, _columnDefinitionCache[mappedField.ArtifactID].OverlayMergeValues))
				{
					queryChunk = choicesImportSql.MergeMultiChoicesQuery(mappedField.CodeTypeID, codeArtifactTableName, mappedField.ArtifactID);
				}
				else
				{
					queryChunk = choicesImportSql.ReplaceChoicesQuery(mappedField.CodeTypeID, codeArtifactTableName, mappedField.ArtifactID);
				}

				if ((int?)mappedField.ImportBehavior == (int?)FieldInfo.ImportBehaviorChoice.ChoiceFieldIgnoreDuplicates
					&& _toggleProvider.IsEnabled<IgnoreDuplicateValuesForMassImportChoice>())
				{
					queryChunk = queryChunk.Replace("/* IncludeDistinctClause */", "DISTINCT");
				}

				var query = new PrintSectionQuery(new InlineSqlQuery($"{queryChunk}"), $"LinkCodeTypeId={mappedField.CodeTypeID}");
				queryChunks.Add(query);
			}

			if (queryChunks.Any())
			{
				var linkingQueries = new StatisticsTimeOnQuery(new SerialSqlQuery(queryChunks));
				var queryToExecute = new IfQuery(
					choicesImportSql.NotAllChoicesAreValidCondition(),
					truePart: choicesImportSql.RaiseInvalidChoicesError(),
					falsePart: linkingQueries);
				using (new QueryMetricsCollector(_context, _importMeasurements))
				{
					_context.ExecuteNonQuerySQLStatement(queryToExecute.BuildQuery(), _queryTimeoutInSeconds);
				}
			}

			InjectionManager.Instance.Evaluate("8e8bdb63-8adc-4db5-af8e-c1f54fe83ee0");
			_importMeasurements.StopMeasure();
			_importMeasurements.SecondaryArtifactCreationTime.Stop();
		}
	}
}
