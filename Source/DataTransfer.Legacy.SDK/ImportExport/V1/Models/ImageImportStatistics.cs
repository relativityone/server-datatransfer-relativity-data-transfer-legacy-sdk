namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class ImageImportStatistics : ImportStatistics
	{
		public bool ExtractedTextReplaced { get; set; }

		public bool SupportImageAutoNumbering { get; set; }

		public int DestinationProductionArtifactID { get; set; }

		public int ExtractedTextDefaultEncodingCodePageID { get; set; }

		public override string ToString()
		{
			return this.ToSafeString();
		}
	}
}