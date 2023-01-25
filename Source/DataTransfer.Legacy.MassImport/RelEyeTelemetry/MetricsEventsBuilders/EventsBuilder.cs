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
			@event.Attributes[Constants.AttributeNames.RunID] = settings.RunID;
			@event.Attributes[Constants.AttributeNames.ImportObjectType] = importType;
			@event.Attributes[Constants.AttributeNames.Billable] = settings.Billable;
			@event.Attributes[Constants.AttributeNames.CustomerApplicationName] = settings.ExecutionSource.ToString();
			@event.Attributes[Constants.AttributeNames.OverlayMode] = settings.Overlay.ToString();
			@event.Attributes[Constants.AttributeNames.OverlayKeyField] = settings.OverlayArtifactID;
			@event.Attributes[Constants.AttributeNames.HasPDF] = settings.HasPDF;
			@event.Attributes[Constants.AttributeNames.AuditLevel] = settings.AuditLevel.ToString();
			return @event;
		}

		public EventJobStart BuildJobStartEvent(Relativity.MassImport.DTO.NativeLoadInfo settings, string importType)
		{
			var @event = new EventJobStart();
			@event.Attributes[Constants.AttributeNames.RunID] = settings.RunID;
			@event.Attributes[Constants.AttributeNames.ImportObjectType] = importType;
			@event.Attributes[Constants.AttributeNames.Billable] = settings.Billable;
			@event.Attributes[Constants.AttributeNames.CustomerApplicationName] = settings.ExecutionSource.ToString();
			@event.Attributes[Constants.AttributeNames.OverlayMode] = settings.Overlay.ToString();
			@event.Attributes[Constants.AttributeNames.OverlayKeyField] = settings.OverlayArtifactID;
			@event.Attributes[Constants.AttributeNames.AuditLevel] = settings.AuditLevel.ToString();

			@event.Attributes[Constants.AttributeNames.MappedFieldsCount] = settings.MappedFields.Length;
			int numberOfFullTextFields = settings.MappedFields.Count(x => x.Category == FieldCategory.FullText);
			int numberOfDataGridFields = settings.MappedFields.Count(f => f.EnableDataGrid);
			int numberOfOffTableTextFields = settings.MappedFields.Count(x => x.Type == FieldTypeHelper.FieldType.OffTableText);
			int numberOfSingleObjectFields = settings.MappedFields.Count(x => x.Type == FieldTypeHelper.FieldType.Object);
			int numberOfMultiObjectFields = settings.MappedFields.Count(x => x.Type == FieldTypeHelper.FieldType.Objects);
			int numberOfSingleChoiceFields = settings.MappedFields.Count(x => x.Type == FieldTypeHelper.FieldType.Code);
			int numberOfMultiChoiceFields = settings.MappedFields.Count(x => x.Type == FieldTypeHelper.FieldType.MultiCode);

			@event.Attributes[Constants.AttributeNames.MappedFullTextCount] = numberOfFullTextFields;
			@event.Attributes[Constants.AttributeNames.MappedDataGridCount] = numberOfDataGridFields;
			@event.Attributes[Constants.AttributeNames.MappedOffTableCount] = numberOfOffTableTextFields;
			@event.Attributes[Constants.AttributeNames.MappedSingleObjectCount] = numberOfSingleObjectFields;
			@event.Attributes[Constants.AttributeNames.MappedMultiObjectCount] = numberOfMultiObjectFields;
			@event.Attributes[Constants.AttributeNames.MappedSingleChoiceCount] = numberOfSingleChoiceFields;
			@event.Attributes[Constants.AttributeNames.MappedMultiChoiceCount] = numberOfMultiChoiceFields;

			return @event;
		}

		public EventJobBatchCompleted BuildJobBatchCompletedEvent(MassImportManagerBase.MassImportResults results, string importType)
		{
			var @event = new EventJobBatchCompleted();
			@event.Attributes[Constants.AttributeNames.RunID] = results.RunID;
			@event.Attributes[Constants.AttributeNames.JobStatus] = results.ExceptionDetail is null ? "success" : "failed";
			@event.Attributes[Constants.AttributeNames.ArtifactsCreatedCount] = results.ArtifactsCreated;
			@event.Attributes[Constants.AttributeNames.ArtifactsUpdatedCount] = results.ArtifactsUpdated;
			@event.Attributes[Constants.AttributeNames.FilesProcessedCount] = results.FilesProcessed;

			return @event;
		}

		public EventGeneralStatistics BuildGeneralStatisticsEvent(string runID, int workspaceID)
		{
			var @event = new EventGeneralStatistics();
			@event.Attributes[Constants.AttributeNames.RunID] = runID;
			@event.Attributes[Constants.AttributeNames.R1WorkspaceID] = workspaceID;

			return @event;
		}
	}
}