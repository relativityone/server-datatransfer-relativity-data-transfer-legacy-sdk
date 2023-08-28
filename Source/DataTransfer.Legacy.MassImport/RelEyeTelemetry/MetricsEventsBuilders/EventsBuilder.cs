// <copyright file="EventsBuilder.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using Relativity;
using System.Linq;

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry.MetricsEventsBuilders
{
	using DataTransfer.Legacy.MassImport.RelEyeTelemetry.Events;
	using Relativity.Core.Service;

	/// <summary>
	/// EventBuilders class.
	/// </summary>
	public class EventsBuilder : IEventsBuilder
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EventsBuilder"/> class.
		/// </summary>
		public EventsBuilder()
		{
		}

		/// <inheritdoc/>
		public EventJobStart BuildJobStartEvent(Relativity.MassImport.DTO.ImageLoadInfo settings, string importType)
		{
			var @event = new EventJobStart();
			@event.Attributes[TelemetryConstants.AttributeNames.RunID] = settings.RunID;
			@event.Attributes[TelemetryConstants.AttributeNames.DataSourceID] = settings.RunID;
			@event.Attributes[TelemetryConstants.AttributeNames.ImportObjectType] = importType;
			@event.Attributes[TelemetryConstants.AttributeNames.Billable] = settings.Billable;
			@event.Attributes[TelemetryConstants.AttributeNames.CustomerApplicationName] = settings.ExecutionSource.ToString();
			@event.Attributes[TelemetryConstants.AttributeNames.OverlayMode] = settings.Overlay.ToString();
			@event.Attributes[TelemetryConstants.AttributeNames.OverlayKeyField] = settings.OverlayArtifactID;
			@event.Attributes[TelemetryConstants.AttributeNames.HasPDF] = settings.HasPDF;
			@event.Attributes[TelemetryConstants.AttributeNames.AuditLevel] = settings.AuditLevel.ToString();
			return @event;
		}

		public EventJobStart BuildJobStartEvent(Relativity.MassImport.DTO.NativeLoadInfo settings, string importType)
		{
			var @event = new EventJobStart();
			@event.Attributes[TelemetryConstants.AttributeNames.RunID] = settings.RunID;
			@event.Attributes[TelemetryConstants.AttributeNames.DataSourceID] = settings.RunID;
			@event.Attributes[TelemetryConstants.AttributeNames.ImportObjectType] = importType;
			@event.Attributes[TelemetryConstants.AttributeNames.Billable] = settings.Billable;
			@event.Attributes[TelemetryConstants.AttributeNames.CustomerApplicationName] = settings.ExecutionSource.ToString();
			@event.Attributes[TelemetryConstants.AttributeNames.OverlayMode] = settings.Overlay.ToString();
			@event.Attributes[TelemetryConstants.AttributeNames.OverlayKeyField] = settings.OverlayArtifactID;
			@event.Attributes[TelemetryConstants.AttributeNames.AuditLevel] = settings.AuditLevel.ToString();

			@event.Attributes[TelemetryConstants.AttributeNames.MappedFieldsCount] = settings.MappedFields.Length;
			int numberOfFullTextFields = settings.MappedFields.Count(x => x.Category == FieldCategory.FullText);
			int numberOfDataGridFields = settings.MappedFields.Count(f => f.EnableDataGrid);
			int numberOfOffTableTextFields = settings.MappedFields.Count(x => x.Type == FieldTypeHelper.FieldType.OffTableText);
			int numberOfSingleObjectFields = settings.MappedFields.Count(x => x.Type == FieldTypeHelper.FieldType.Object);
			int numberOfMultiObjectFields = settings.MappedFields.Count(x => x.Type == FieldTypeHelper.FieldType.Objects);
			int numberOfSingleChoiceFields = settings.MappedFields.Count(x => x.Type == FieldTypeHelper.FieldType.Code);
			int numberOfMultiChoiceFields = settings.MappedFields.Count(x => x.Type == FieldTypeHelper.FieldType.MultiCode);

			@event.Attributes[TelemetryConstants.AttributeNames.MappedFullTextCount] = numberOfFullTextFields;
			@event.Attributes[TelemetryConstants.AttributeNames.MappedDataGridCount] = numberOfDataGridFields;
			@event.Attributes[TelemetryConstants.AttributeNames.MappedOffTableCount] = numberOfOffTableTextFields;
			@event.Attributes[TelemetryConstants.AttributeNames.MappedSingleObjectCount] = numberOfSingleObjectFields;
			@event.Attributes[TelemetryConstants.AttributeNames.MappedMultiObjectCount] = numberOfMultiObjectFields;
			@event.Attributes[TelemetryConstants.AttributeNames.MappedSingleChoiceCount] = numberOfSingleChoiceFields;
			@event.Attributes[TelemetryConstants.AttributeNames.MappedMultiChoiceCount] = numberOfMultiChoiceFields;

			return @event;
		}

		public EventJobBatchCompleted BuildJobBatchCompletedEvent(MassImportManagerBase.MassImportResults results, string importType)
		{
			var @event = new EventJobBatchCompleted();
			@event.Attributes[TelemetryConstants.AttributeNames.RunID] = results.RunID;
			@event.Attributes[TelemetryConstants.AttributeNames.DataSourceID] = results.RunID;
			@event.Attributes[TelemetryConstants.AttributeNames.JobStatus] = results.ExceptionDetail is null ? "success" : "failed";
			@event.Attributes[TelemetryConstants.AttributeNames.ArtifactsCreatedCount] = results.ArtifactsCreated;
			@event.Attributes[TelemetryConstants.AttributeNames.ArtifactsUpdatedCount] = results.ArtifactsUpdated;
			@event.Attributes[TelemetryConstants.AttributeNames.FilesProcessedCount] = results.FilesProcessed;

			return @event;
		}

		public EventGeneralStatistics BuildGeneralStatisticsEvent(string runID, int workspaceID)
		{
			var @event = new EventGeneralStatistics();
			@event.Attributes[TelemetryConstants.AttributeNames.RunID] = runID;
			@event.Attributes[TelemetryConstants.AttributeNames.DataSourceID] = runID;
			@event.Attributes[TelemetryConstants.AttributeNames.R1WorkspaceID] = workspaceID;

			return @event;
		}
	}
}