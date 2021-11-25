using System.Collections.Generic;

namespace Relativity.MassImport.Core.Pipeline.Stages.Job.PopulateStagingTables
{
	internal class MassImportReaderCodeTempTableReader : kCura.Data.RowDataGateway.SqlBulkCopyDataReader
	{
		private readonly MassImportCodeTempTableRow[] _massCreateCodeTempTableRows;
		private int _rowNumber;

		public MassImportReaderCodeTempTableReader(IEnumerable<System.Data.SqlClient.SqlBulkCopyColumnMapping> columnMappings, MassImportCodeTempTableRow[] massCreateTableRows) : base(columnMappings)
		{
			_massCreateCodeTempTableRows = massCreateTableRows;
			_rowNumber = 0;
		}

		public override bool Read()
		{
			bool retVal = _rowNumber < _massCreateCodeTempTableRows.Length;
			_rowNumber += 1;
			return retVal;
		}

		public override object GetColumnValue(int i)
		{
			var retVal = new object();
			switch (i)
			{
				case 0:
					{
						retVal = _massCreateCodeTempTableRows[_rowNumber - 1].ArtifactIdentifier;
						break;
					}

				case 1:
					{
						retVal = (object)_massCreateCodeTempTableRows[_rowNumber - 1].CodeArtifactID;
						break;
					}

				case 2:
					{
						retVal = (object)_massCreateCodeTempTableRows[_rowNumber - 1].CodeTypeID;
						break;
					}
			}

			return retVal;
		}
	}
}