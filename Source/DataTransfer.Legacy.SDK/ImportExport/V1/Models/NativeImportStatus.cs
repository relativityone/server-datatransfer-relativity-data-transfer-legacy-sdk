namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class NativeImportStatus
	{
		public int ArtifactId { get; set; }

		public string Identifier { get; set; }

		public long Status { get; set; }

		public int OriginalLineNumber { get; set; }
	}
}