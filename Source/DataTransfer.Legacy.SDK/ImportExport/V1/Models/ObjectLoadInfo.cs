namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class ObjectLoadInfo : NativeLoadInfo
	{
		public int ArtifactTypeID { get; set; }

		public override string ToString()
		{
			return this.ToSafeString();
		}
	}
}