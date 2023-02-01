// <copyright file="Constants.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry
{
	/// <summary>
	/// Constants for RelEye telemetry.
	/// </summary>
	public static class TelemetryConstants
	{
		/// <summary>
		/// Values for Import Service attributes.
		/// </summary>
		public static class Values
		{
			/// <summary>
			/// Service name.
			/// </summary>
			public const string ServiceName = "data-transfer-legacy-rap-kepler-api";

			/// <summary>
			/// Service namespace.
			/// </summary>
			public const string ServiceNamespace = "data-transfer-legacy-rap";

			/// <summary>
			/// Name of the DataTransfer.Legacy.
			/// </summary>
			public const string ApplicationName = "DataTransfer.Legacy";

			/// <summary>
			/// Guid of the Application
			/// </summary>
			public const string ApplicationID = "9f9d45ff-5dcd-462d-996d-b9033ea8cfce";
			/// <summary>
			/// Team identification in backstage.
			/// </summary>
			public const string R1TeamID = "PTCI-4941411";
		}

		/// <summary>
		/// RelEye attributes names.
		/// </summary>
		public static class AttributeNames
		{
			/// <summary>
			/// [string] Application name.
			/// </summary>
			public const string ApplicationName = "application.name";

			/// <summary>
			/// [string] Application name.
			/// </summary>
			public const string ApplicationID = "application.guid";

			/// <summary>
			/// [string] Correlation id.
			/// </summary>
			public const string CorrelationID = "CorrelationID";

			/// <summary>
			/// [int] Number of artifacts created.
			/// </summary>
			public const string ArtifactsCreatedCount = "import.artifacts_created_count";

			/// <summary>
			/// [int] Number of artifacts updated.
			/// </summary>
			public const string ArtifactsUpdatedCount = "import.artifacts_updated_count";
			
			/// <summary>
			/// [int] Number of choices created.
			/// </summary>
			public const string ChoicesCreatedCount = "import.choices_created_count";

			/// <summary>
			/// [int] Number of documents created.
			/// </summary>
			public const string DocumentsCreatedCount = "import.documents_created_count";

			/// <summary>
			/// [int] Number of documents updated.
			/// </summary>
			public const string DocumentsUpdatedCount = "import.documents_updated_count";

			/// <summary>
			/// [int] Number of errors.
			/// </summary>
			public const string ErrorsCount = "import.errors_count";
			
			/// <summary>
			/// [int] Number of files loaded.
			/// </summary>
			public const string FilesLoadedCount = "import.files_loaded_count";

			/// <summary>
			/// [int] Number of files processed.
			/// </summary>
			public const string FilesProcessedCount = "import.files_processed_count";
			
			/// <summary>
			/// [int] Number of folders processed.
			/// </summary>
			public const string FoldersCreatedCount = "import.folders_created_count";

			/// <summary>
			/// [string] Customer application name.
			/// </summary>
			public const string CustomerApplicationName = "import.customer_application_name";

			/// <summary>
			/// [string] Audit level set for import: FullAudit, NoSnapshot, NoAudit.
			/// </summary>
			public const string AuditLevel = "import.settings.audit_level";

			/// <summary>
			/// [bool] Indicate whether import is billable.
			/// </summary>
			public const string Billable = "import.settings.billable";

			/// <summary>
			/// [int] Number of mapped fields.
			/// </summary>
			public const string MappedFieldsCount = "import.settings.fields.mapped.count";

			/// <summary>
			/// [int] Number of mapped data grid fields.
			/// </summary>
			public const string MappedDataGridCount = "import.settings.fields.data_grid.count";
			
			/// <summary>
			/// [int] Number of mapped full text fields.
			/// </summary>
			public const string MappedFullTextCount = "import.settings.fields.full_text.count";
			
			/// <summary>
			/// [int] Number of mapped multi choice fields.
			/// </summary>
			public const string MappedMultiChoiceCount = "import.settings.fields.multi_choice.count";
			
			/// <summary>
			/// [int] Number of mapped multi object fields.
			/// </summary>
			public const string MappedMultiObjectCount = "import.settings.fields.multi_object.count";
			
			/// <summary>
			/// [int] Number of mapped off table fields.
			/// </summary>
			public const string MappedOffTableCount = "import.settings.fields.off_table.count";
			
			/// <summary>
			/// [int] Number of mapped single choice fields.
			/// </summary>
			public const string MappedSingleChoiceCount = "import.settings.fields.single_choice.count";

			/// <summary>
			/// [int] Number of mapped single object fields.
			/// </summary>
			public const string MappedSingleObjectCount = "import.settings.fields.single_object.count";

			/// <summary>
			/// [bool] .
			/// </summary>
			public const string HasPDF = "import.settings.image.has_pdf";

			/// <summary>
			/// [int] Overlay key field.
			/// </summary>
			public const string OverlayKeyField = "import.settings.overlay.keyfield";

			/// <summary>
			/// [string] Overlay mode, eg. append, overlay, appendoverlay.
			/// </summary>
			public const string OverlayMode = "import.settings.overlay.mode";

			/// <summary>
			/// [string] Type of the import.
			/// </summary>
			public const string ImportObjectType = "import.type";

			/// <summary>
			/// [int] Number of warnings.
			/// </summary>
			public const string WarningsCount = "import.warnings_count";

			/// <summary>
			/// [string] Job status.
			/// </summary>
			public const string JobStatus = "job.result";

			/// <summary>
			/// [string] Team identification.
			/// </summary>
			public const string R1TeamID = "r1.team.id";

			/// <summary>
			/// [string] R1 version.
			/// </summary>
			public const string RelativityVersion = "r1.version";

			/// <summary>
			/// [int] Workspace id.
			/// </summary>
			public const string R1WorkspaceID = "r1.workspace.id";

			/// <summary>
			/// [string] Import Run Identifier (from Import Service API).
			/// </summary>
			public const string RunID = "import.run_id";

			/// <summary>
			/// [string] Service name.
			/// </summary>
			public const string ServiceName = "service.name";

			/// <summary>
			/// [string] Service namespace.
			/// </summary>
			public const string ServiceNamespace = "service.namespace";

			/// <summary>
			/// [string] Version of the Import Service.
			/// </summary>
			public const string ServiceVersion = "service.version";

			/// <summary>
			/// [string] Status of the operation, like success or failed.
			/// </summary>
			public const string Status = "status.code";

			/// <summary>
			/// [string] job trigger
			/// </summary>
			public const string ExecutionSource = "job.trigger";

			/// <summary>
			/// [string] job workflow_id
			/// </summary>
			public const string BatchId = "job.workflow_id";

			/// <summary>
			/// [string] job type
			/// </summary>
			public const string JobType = "job.type";
		}

		/// <summary>
		/// Event names.
		/// </summary>
		public class EventName
		{
			/// <summary>
			/// Job start event name.
			/// </summary>
			public const string JobStart = "job_start";

			/// <summary>
			/// Job batch event name.
			/// </summary>
			public const string JobBatchCompleted = "job_batch_completed";

			/// <summary>
			/// Job statistics event name.
			/// </summary>
			public const string GeneralStatistics = "general_statistics";

		}
	}
}
