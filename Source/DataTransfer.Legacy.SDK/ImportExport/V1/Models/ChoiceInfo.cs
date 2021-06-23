namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class ChoiceInfo
	{
		public ChoiceInfo()
		{
			Name = string.Empty;
		}

		public int Order { get; set; }

		public int CodeTypeID { get; set; }

		[SensitiveData]
		public string Name { get; set; }

		public int ArtifactID { get; set; }

		public int ParentArtifactID { get; set; }

		public override string ToString()
		{
			return this.ToSafeString();
		}
	}
}