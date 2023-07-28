namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class NativeLoadInfo
	{
		public NativeLoadInfo()
		{
			OverlayArtifactID = -1;
			Billable = true;
		}

		public LoadRange Range { get; set; }

		public FieldInfo[] MappedFields { get; set; }

		public OverwriteType Overlay { get; set; }

		public string Repository { get; set; }

		public string RunID { get; set; }

		public string DataFileName { get; set; }

		public bool UseBulkDataImport { get; set; }

		public bool UploadFiles { get; set; }

		public string CodeFileName { get; set; }

		public string ObjectFileName { get; set; }

		public string DataGridFileName { get; set; }

		public string DataGridOffsetFileName { get; set; }

		public bool DisableUserSecurityCheck { get; set; }

		public string OnBehalfOfUserToken { get; set; }

		public ImportAuditLevel AuditLevel { get; set; }

		public string BulkLoadFileFieldDelimiter { get; set; }

		public int OverlayArtifactID { get; set; }

		public OverlayBehavior OverlayBehavior { get; set; }

		public bool LinkDataGridRecords { get; set; }

		public bool LoadImportedFullTextFromServer { get; set; }

		public int KeyFieldArtifactID { get; set; }

		public int RootFolderID { get; set; }

		public bool MoveDocumentsInAppendOverlayMode { get; set; }

		public ExecutionSource ExecutionSource { get; set; }

		public bool Billable { get; set; }

		public string BulkFileSharePath { get; set; }

		public override string ToString()
		{
			return this.ToSafeString();
		}
	}
}