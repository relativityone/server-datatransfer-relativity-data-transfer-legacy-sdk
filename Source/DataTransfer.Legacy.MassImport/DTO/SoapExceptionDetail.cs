using System;
using System.Collections.Generic;

namespace Relativity.MassImport.DTO
{
	[System.Xml.Serialization.XmlType("SoapExceptionDetail")]
	[System.Xml.Serialization.XmlRoot(ElementName = "detail")]
	[Serializable()]
	public class SoapExceptionDetail
	{
		public SoapExceptionDetail()
		{
		}

		public SoapExceptionDetail(Exception ex)
		{
			ExceptionType = ex.GetType().ToString();
			SetMessageText(ex);
			ExceptionTrace = null;
			ExceptionFullText = ExceptionMessage;
		}

		private void SetMessageText(Exception ex)
		{
			var stringBuilder = new System.Text.StringBuilder();
			GetBaseMessageAndAllInnerMessages(ex, stringBuilder);
			ExceptionMessage = stringBuilder.ToString();
		}

		[System.Xml.Serialization.XmlElement("ExceptionType")]
		public string ExceptionType { get; set; }
		[System.Xml.Serialization.XmlElement("ExceptionMessage")]
		public string ExceptionMessage { get; set; }
		[System.Xml.Serialization.XmlElement("ExceptionTrace")]
		public string ExceptionTrace { get; set; }
		[System.Xml.Serialization.XmlElement("ExceptionFullText")]
		public string ExceptionFullText { get; set; }
		[System.Xml.Serialization.XmlElement("Details")]
		public List<string> Details { get; set; } = new List<string>();

		private System.Text.StringBuilder GetBaseMessageAndAllInnerMessages(Exception ex, System.Text.StringBuilder sb)
		{
			sb.AppendLine("Error: " + ex.Message);
			if (ex.InnerException is object)
			{
				sb.AppendLine("---Additional Errors---");
				GetBaseMessageAndAllInnerMessages(ex.InnerException, sb);
			}

			return sb;
		}
	}
}