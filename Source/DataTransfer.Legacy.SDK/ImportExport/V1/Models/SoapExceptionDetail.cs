using System.Collections.Generic;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class SoapExceptionDetail
	{
		public SoapExceptionDetail()
		{
			Details = new List<string>();
		}

		public string ExceptionType { get; set; }

		public string ExceptionMessage { get; set; }

		public string ExceptionTrace { get; set; }

		public string ExceptionFullText { get; set; }

		public List<string> Details { get; set; }
	}
}