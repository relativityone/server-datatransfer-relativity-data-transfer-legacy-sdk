namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class IoResponse
	{
		public bool Success { get; set; }

		public string Filename { get; set; }

		public string ErrorMessage { get; set; }

		public string ErrorText { get; set; }

		public override string ToString()
		{
			return this.ToSafeString();
		}
	}
}