namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class LoadRange
	{
		public int StartIndex { get; set; }

		public int Count { get; set; }

		public override string ToString()
		{
			return this.ToSafeString();
		}
	}
}