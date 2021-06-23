namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class ObjectImportStatistics : ImportStatistics
	{
		public int ArtifactTypeID { get; set; }

		public char Delimiter { get; set; }

		public char Bound { get; set; }

		public char NewlineProxy { get; set; }

		public char MultiValueDelimiter { get; set; }

		public int LoadFileEncodingCodePageID { get; set; }

		public int ExtractedTextFileEncodingCodePageID { get; set; }

		public string FolderColumnName { get; set; }

		public string FileFieldColumnName { get; set; }

		public bool ExtractedTextPointsToFile { get; set; }

		public int NumberOfChoicesCreated { get; set; }

		public int NumberOfFoldersCreated { get; set; }

		public int[][] FieldsMapped { get; set; }

		public char NestedValueDelimiter { get; set; }

		public override string ToString()
		{
			return this.ToSafeString();
		}
	}
}