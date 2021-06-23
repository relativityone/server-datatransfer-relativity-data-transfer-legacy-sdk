namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class RelationalFieldPane
	{
		public int PaneOrder { get; set; }

		public string IconFilename { get; set; }

		public string ColumnName { get; set; }

		public int FieldArtifactID { get; set; }

		public int RelationalViewArtifactID { get; set; }

		public string HeaderText { get; set; }

		public byte[] IconFileData { get; set; }

		public int PaneID { get; set; }

		public override string ToString()
		{
			return this.ToSafeString();
		}
	}
}