namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class ImportStatistics
	{
		public int[] BatchSizes { get; set; }

		public RepositoryConnectionType RepositoryConnection { get; set; }

		public OverwriteType Overwrite { get; set; }

		public int OverlayIdentifierFieldArtifactID { get; set; }

		public int DestinationFolderArtifactID { get; set; }

		public string LoadFileName { get; set; }

		public int StartLine { get; set; }

		public string FilesCopiedToRepository { get; set; }

		public long TotalFileSize { get; set; }

		public long TotalMetadataBytes { get; set; }

		public int NumberOfDocumentsCreated { get; set; }

		public int NumberOfDocumentsUpdated { get; set; }

		public int NumberOfFilesLoaded { get; set; }

		public long NumberOfErrors { get; set; }

		public long NumberOfWarnings { get; set; }

		public int RunTimeInMilliseconds { get; set; }

		public bool SendNotification { get; set; }

		public OverlayBehavior? OverlayBehavior { get; set; }
	}
}