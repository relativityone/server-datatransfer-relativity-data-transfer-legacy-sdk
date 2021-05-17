namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class FieldInfo
	{
		public FieldInfo()
		{
			FormatString = string.Empty;
		}

		public int ArtifactID { get; set; }

		public FieldCategory Category { get; set; }

		public FieldType Type { get; set; }

		public string DisplayName { get; set; }

		public int TextLength { get; set; }

		public int CodeTypeID { get; set; }

		public bool EnableDataGrid { get; set; }

		public string FormatString { get; set; }

		public bool IsUnicodeEnabled { get; set; }

		public ImportBehaviorChoice? ImportBehavior { get; set; }
	}
}