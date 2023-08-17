namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class ImageLoadInfo
	{
		public ImageLoadInfo()
		{
			OverlayArtifactID = -1;
			Billable = true;
		}

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

		public ImportAuditLevel AuditLevel { get; set; }

		public int OverlayArtifactID { get; set; }

		public ExecutionSource ExecutionSource { get; set; }

		public bool Billable { get; set; }

		public string BulkFileSharePath { get; set; }

        public bool OverrideReferentialLinksRestriction { get; set; }

        public bool HasPDF { get; set; }

        public override string ToString()
		{
			return this.ToSafeString();
		}
	}
}