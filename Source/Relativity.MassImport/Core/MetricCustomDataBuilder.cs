using System.Collections.Generic;
using System.Linq;
using Relativity.Core.Service;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.Core
{
	internal class MetricCustomDataBuilder
	{
		private readonly Dictionary<string, object> _customData;

		private MetricCustomDataBuilder()
		{
			_customData = new Dictionary<string, object>();
		}

		public static MetricCustomDataBuilder New()
		{
			return new MetricCustomDataBuilder();
		}

		public MetricCustomDataBuilder WithContext(string importType, string system)
		{
			_customData["ImportType"] = importType;
			_customData["System"] = system;
			_customData["MassImportImprovementsToggle"] = true;

			return this;
		}

		public MetricCustomDataBuilder WithSettings(NativeLoadInfo settings)
		{
			// Import settings
			_customData[nameof(settings.ExecutionSource)] = settings.ExecutionSource.ToString();
			_customData[nameof(settings.AuditLevel)] = settings.AuditLevel.ToString();
			_customData[nameof(settings.Overlay)] = settings.Overlay.ToString();
			_customData[nameof(settings.OverlayBehavior)] = settings.OverlayBehavior.ToString();
			_customData[nameof(settings.OverlayArtifactID)] = settings.OverlayArtifactID;
			_customData[nameof(settings.DisableUserSecurityCheck)] = settings.DisableUserSecurityCheck;
			_customData[nameof(settings.UploadFiles)] = settings.UploadFiles;
			_customData[nameof(settings.UseBulkDataImport)] = settings.UseBulkDataImport;
			_customData[nameof(settings.MoveDocumentsInAppendOverlayMode)] = settings.MoveDocumentsInAppendOverlayMode;
			_customData[nameof(settings.LinkDataGridRecords)] = settings.LinkDataGridRecords;
			_customData[nameof(settings.LoadImportedFullTextFromServer)] = settings.LoadImportedFullTextFromServer;
			_customData[nameof(settings.KeyFieldArtifactID)] = settings.KeyFieldArtifactID;
			_customData[nameof(settings.RootFolderID)] = settings.RootFolderID;
			_customData[nameof(settings.Billable)] = settings.Billable;
			_customData[$"{nameof(settings.Range)}Defined"] = settings.Range != null;
			_customData[$"{nameof(settings.Range)}Start"] = settings.Range?.StartIndex;
			_customData[$"{nameof(settings.Range)}Count"] = settings.Range?.Count;

			// Mapped fields
			_customData[nameof(settings.MappedFields)] = settings.MappedFields.Length;
			int NumberOfFullTextFields = settings.MappedFields.Count(x => x.Category == FieldCategory.FullText);
			int NumberOfDataGridFields = settings.MappedFields.Count(f => f.EnableDataGrid);
			int NumberOfOffTableTextFields = settings.MappedFields.Count(x => x.Type == FieldTypeHelper.FieldType.OffTableText);
			int NumberOfSingleObjectFields = settings.MappedFields.Count(x => x.Type == FieldTypeHelper.FieldType.Object);
			int NumberOfMultiObjectFields = settings.MappedFields.Count(x => x.Type == FieldTypeHelper.FieldType.Objects);
			int NumberOfSingleChoiceFields = settings.MappedFields.Count(x => x.Type == FieldTypeHelper.FieldType.Code);
			int NumberOfMultiChoiceFields = settings.MappedFields.Count(x => x.Type == FieldTypeHelper.FieldType.MultiCode);
			bool IsFolderNameMapped = settings.MappedFields.Any(x => x.Category == FieldCategory.FolderName);
			_customData[nameof(NumberOfFullTextFields)] = NumberOfFullTextFields;
			_customData[nameof(NumberOfDataGridFields)] = NumberOfDataGridFields;
			_customData[nameof(NumberOfOffTableTextFields)] = NumberOfOffTableTextFields;
			_customData[nameof(NumberOfSingleObjectFields)] = NumberOfSingleObjectFields;
			_customData[nameof(NumberOfMultiObjectFields)] = NumberOfMultiObjectFields;
			_customData[nameof(NumberOfSingleChoiceFields)] = NumberOfSingleChoiceFields;
			_customData[nameof(NumberOfMultiChoiceFields)] = NumberOfMultiChoiceFields;
			_customData[nameof(IsFolderNameMapped)] = IsFolderNameMapped;

			// Object import
			if (settings is ObjectLoadInfo settingsAsObjectLoadInfo)
			{
				_customData[nameof(settingsAsObjectLoadInfo.ArtifactTypeID)] = settingsAsObjectLoadInfo.ArtifactTypeID;
			}

			return this;
		}

		public MetricCustomDataBuilder WithSettings(ImageLoadInfo settings)
		{
			_customData[nameof(settings.ExecutionSource)] = settings.ExecutionSource.ToString();
			_customData[nameof(settings.AuditLevel)] = settings.AuditLevel.ToString();
			_customData[nameof(settings.Overlay)] = settings.Overlay.ToString();
			_customData[nameof(settings.DisableUserSecurityCheck)] = settings.DisableUserSecurityCheck;
			_customData[nameof(settings.UseBulkDataImport)] = settings.UseBulkDataImport;
			_customData[nameof(settings.UploadFullText)] = settings.UploadFullText;
			_customData[nameof(settings.KeyFieldArtifactID)] = settings.KeyFieldArtifactID;
			_customData[nameof(settings.OverlayArtifactID)] = settings.OverlayArtifactID;
			_customData[nameof(settings.DestinationFolderArtifactID)] = settings.DestinationFolderArtifactID;
			_customData[nameof(settings.Billable)] = settings.Billable;

			return this;
		}

		public MetricCustomDataBuilder WithResult(IMassImportManagerInternal.MassImportResults results)
		{
			_customData["IsSuccess"] = results.ExceptionDetail is null;
			_customData[nameof(results.ArtifactsCreated)] = results.ArtifactsCreated;
			_customData[nameof(results.ArtifactsUpdated)] = results.ArtifactsUpdated;
			_customData[nameof(results.FilesProcessed)] = results.FilesProcessed;

			return this;
		}

		public MetricCustomDataBuilder WithMeasurements(ImportMeasurements importMeasurements)
		{
			_customData[nameof(importMeasurements.SqlImportTime)] = importMeasurements.SqlImportTime.ElapsedMilliseconds;
			_customData[nameof(importMeasurements.SqlBulkImportTime)] = importMeasurements.SqlBulkImportTime.ElapsedMilliseconds;
			_customData[nameof(importMeasurements.DataGridFileSize)] = importMeasurements.DataGridFileSize;
			_customData[nameof(importMeasurements.DataGridImportTime)] = importMeasurements.DataGridImportTime.ElapsedMilliseconds;

			foreach (KeyValuePair<string, long> additionalMeasurement in importMeasurements.GetMeasures())
			{
				_customData[additionalMeasurement.Key] = additionalMeasurement.Value;
			}

			foreach (KeyValuePair<string, int> counter in importMeasurements.GetCounters())
			{
				_customData[counter.Key] = counter.Value;
			}

			return this;
		}

		public MetricCustomDataBuilder WithChoicesDetails(IDictionary<int, int> numberOfChoicesPerCodeTypeId)
		{
			int totalNumberOfChoices = 0;
			foreach (KeyValuePair<int, int> codeTypeIdToNumberOfChoices in numberOfChoicesPerCodeTypeId)
			{
				int codeTypeId = codeTypeIdToNumberOfChoices.Key;
				int numberOfChoices = codeTypeIdToNumberOfChoices.Value;

				totalNumberOfChoices += numberOfChoices;
				_customData[$"NumberOfChoices_{codeTypeId}"] = numberOfChoices;
			}

			_customData["NumberOfChoices"] = totalNumberOfChoices;

			return this;
		}

		public Dictionary<string, object> Build()
		{
			return _customData;
		}
	}
}
