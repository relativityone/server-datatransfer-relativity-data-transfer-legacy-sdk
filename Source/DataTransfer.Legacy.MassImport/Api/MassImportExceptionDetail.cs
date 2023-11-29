using System;
using System.Collections.Generic;
using System.Text;

namespace Relativity.MassImport.Api
{
	public class MassImportExceptionDetail
	{
		public MassImportExceptionDetail()
		{
		}

		public MassImportExceptionDetail(Exception ex)
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

		public string ExceptionType { get; set; }
		public string ExceptionMessage { get; set; }
		public string ExceptionTrace { get; set; }
		public string ExceptionFullText { get; set; }
		public List<string> Details { get; set; } = new List<string>();

		private void GetBaseMessageAndAllInnerMessages(Exception ex, StringBuilder sb)
		{
			sb.AppendLine("Error: " + ex.Message);
			if (ex.InnerException is object)
			{
				sb.AppendLine("---Additional Errors---");
				GetBaseMessageAndAllInnerMessages(ex.InnerException, sb);
			}
		}
	}
}