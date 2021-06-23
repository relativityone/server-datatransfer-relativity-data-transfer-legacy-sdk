using System;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class InitializationResults
	{
		public Guid RunId { get; set; }

		public long RowCount { get; set; }

		public string[] ColumnNames { get; set; }

		public override string ToString()
		{
			return this.ToSafeString();
		}
	}
}