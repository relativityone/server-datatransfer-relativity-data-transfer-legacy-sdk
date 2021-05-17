namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class Code : Artifact
	{
		public Code()
		{
			ArtifactTypeID = 7;
			RelativityApplications = new int[0];
			Name = string.Empty;
		}

		public int CodeType { get; set; }

		public string Name { get; set; }

		public int Order { get; set; }

		public bool IsActive { get; set; }

		public bool UpdateInSearchEngine { get; set; }

		public int? OIHiliteStyleID { get; set; }

		public KeyboardShortcut KeyboardShortcut { get; set; }

		public int[] RelativityApplications { get; set; }
	}
}