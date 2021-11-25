using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Relativity.Core.Service;
using Relativity.Data.MassImport;
using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Framework.Stages;
using Relativity.MassImport.Core.Pipeline.Input.Interface;
using Relativity.Telemetry.APM;

namespace Relativity.MassImport.Core.Pipeline.Stages.Shared
{
	/// <summary>
	/// This stage is responsible for sending legacy mass import metrics.
	/// Dashboard in New Relic: "Import metrics".
	/// </summary>
	/// <typeparam name="T">Any input type</typeparam>
	internal class SendLegacyImportMetricDecoratorStage<T> : DecoratorStage<T, IMassImportManagerInternal.MassImportResults>
		where T : IImportSettingsInput<NativeLoadInfo>
	{
		private readonly MassImportContext _context;
		private readonly IAPM _apmClient;
		private readonly string _metricName;

		public SendLegacyImportMetricDecoratorStage(
			IPipelineStage<T, IMassImportManagerInternal.MassImportResults> importStage,
			IPipelineExecutor pipelineExecutor,
			MassImportContext context,
			string metricName,
			IAPM apmClient) : base(pipelineExecutor, importStage)
		{
			_apmClient = apmClient;
			_context = context;
			_metricName = metricName;
		}

		public override IMassImportManagerInternal.MassImportResults Execute(T input)
		{
			var settings = input.Settings;

			var returnValue = new IMassImportManagerInternal.MassImportResults(); // TODO comment why it is new

			var importStopWatch = Stopwatch.StartNew();
			try
			{
				returnValue = base.Execute(input);
				return returnValue;
			}
			finally
			{
				importStopWatch.Stop();

				_apmClient.TimedOperation(
					name: $"ImportAPI.{_metricName}.ImportTime",
					correlationID: settings.RunID,
					precalculatedMilliseconds: importStopWatch.ElapsedMilliseconds,
					customData: CreateImportMetricCustomData(settings, returnValue));
				_apmClient.TimedOperation(
					name: $"ImportAPI.{_metricName}.DataGridWriteTime",
					correlationID: settings.RunID,
					precalculatedMilliseconds: _context.ImportMeasurements.DataGridImportTime.ElapsedMilliseconds,
					customData: CreateDataGridImportMetricsCustomData(settings, returnValue,
						_context.ImportMeasurements));
				_apmClient.TimedOperation(
					name: $"ImportAPI.{_metricName}.SqlWriteTime",
					correlationID: settings.RunID,
					precalculatedMilliseconds: _context.ImportMeasurements.SqlImportTime.ElapsedMilliseconds,
					customData: CreateSqlImportMetricsCustomData(settings, returnValue, _context.ImportMeasurements));
			}
		}

		private Dictionary<string, object> CreateDataGridImportMetricsCustomData(
			NativeLoadInfo settings,
			IMassImportManagerInternal.MassImportResults results,
			Data.ImportMeasurements importMeasurements)
		{
			Dictionary<string, object> dict = CreateImportMetricCustomData(settings, results);
			dict.Add(nameof(importMeasurements.DataGridFileSize), importMeasurements.DataGridFileSize);
			return dict;
		}

		private Dictionary<string, object> CreateSqlImportMetricsCustomData(
			NativeLoadInfo settings,
			IMassImportManagerInternal.MassImportResults results,
			Data.ImportMeasurements importMeasurements)
		{
			Dictionary<string, object> dict = CreateImportMetricCustomData(settings, results);
			dict.Add(nameof(settings.AuditLevel), settings.AuditLevel.ToString());
			dict.Add(nameof(settings.Overlay), settings.Overlay.ToString());
			dict.Add(nameof(importMeasurements.SqlBulkImportTime), 
				importMeasurements.SqlBulkImportTime.ElapsedMilliseconds);
			dict.Add(nameof(settings.UseBulkDataImport), settings.UseBulkDataImport);
			dict.Add(nameof(importMeasurements.PrimaryArtifactCreationTime),
				importMeasurements.PrimaryArtifactCreationTime.ElapsedMilliseconds);
			dict.Add(nameof(importMeasurements.SecondaryArtifactCreationTime
			), importMeasurements.SecondaryArtifactCreationTime.ElapsedMilliseconds);

			return dict;
		}

		private Dictionary<string, object> CreateImportMetricCustomData(
			NativeLoadInfo settings,
			IMassImportManagerInternal.MassImportResults results)
		{
			return new Dictionary<string, object>
			{
				["ImportedArtifacts"] = results.ArtifactsCreated + results.ArtifactsUpdated,
				[nameof(settings.ExecutionSource)] = settings.ExecutionSource.ToString(),
				["MappedDataGridFields"] = settings.MappedFields.Count(f => f.EnableDataGrid),
				[nameof(settings.LinkDataGridRecords)] = settings.LinkDataGridRecords,
				[nameof(settings.LoadImportedFullTextFromServer)] = settings.LoadImportedFullTextFromServer
			};
		}
	}
}
