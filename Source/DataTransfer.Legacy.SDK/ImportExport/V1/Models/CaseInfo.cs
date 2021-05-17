namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class CaseInfo
	{
		public int ArtifactID { get; set; }

		public string Name { get; set; }

		public int MatterArtifactID { get; set; }

		public int StatusCodeArtifactID { get; set; }

		public bool EnableDataGrid { get; set; }

		public int RootFolderID { get; set; }

		public int RootArtifactID { get; set; }

		public string DownloadHandlerURL { get; set; }

		public bool AsImportAllowed { get; set; }

		public bool ExportAllowed { get; set; }

		public string DocumentPath { get; set; }
	}
}