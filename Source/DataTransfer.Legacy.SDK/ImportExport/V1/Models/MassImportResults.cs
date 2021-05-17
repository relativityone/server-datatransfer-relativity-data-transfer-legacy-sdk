namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class MassImportResults
	{
		public int FilesProcessed { get; set; }
		public int ArtifactsCreated { get; set; }
		public int ArtifactsUpdated { get; set; }
		public string RunID { get; set; }
		public SoapExceptionDetail ExceptionDetail { get; set; }
	}
}