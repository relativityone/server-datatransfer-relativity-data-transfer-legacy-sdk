using System;

namespace Relativity.MassImport.DTO
{
	[Serializable]
	public class ImageLoadInfo
	{
		public bool DisableUserSecurityCheck { get; set; }

		public string RunID { get; set; }

		public OverwriteType Overlay { get; set; }

		public string Repository { get; set; }

		public bool UseBulkDataImport { get; set; }

		public bool UploadFullText { get; set; }

		public string BulkFileName { get; set; }

		public string DataGridFileName { get; set; }

		public int KeyFieldArtifactID { get; set; }

		public int DestinationFolderArtifactID { get; set; }

		public ImportAuditLevel AuditLevel { get; set; } = ImportAuditLevel.FullAudit;

		public int OverlayArtifactID { get; set; } = -1;

		public ExecutionSource ExecutionSource { get; set; } = ExecutionSource.Unknown;

		public bool Billable { get; set; } = true;
    }
}
