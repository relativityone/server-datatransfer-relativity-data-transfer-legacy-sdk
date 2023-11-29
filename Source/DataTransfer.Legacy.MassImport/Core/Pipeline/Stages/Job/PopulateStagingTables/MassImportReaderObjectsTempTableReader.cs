using System.Collections.Generic;

namespace Relativity.MassImport.Core.Pipeline.Stages.Job.PopulateStagingTables
{
	internal class MassImportReaderObjectsTempTableReader : kCura.Data.RowDataGateway.SqlBulkCopyDataReader
	{
		private readonly MassImportObjectsTempTableRow[] _massCreateObjectsTempTableRows;
		private int _rowNumber;

		public MassImportReaderObjectsTempTableReader(IEnumerable<System.Data.SqlClient.SqlBulkCopyColumnMapping> columnMappings, MassImportObjectsTempTableRow[] massCreateTableRows) : base(columnMappings)
		{
			_massCreateObjectsTempTableRows = massCreateTableRows;
			_rowNumber = 0;
		}

		public override bool Read()
		{
			bool retVal = _rowNumber < _massCreateObjectsTempTableRows.Length;
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
						retVal = _massCreateObjectsTempTableRows[_rowNumber - 1].ArtifactIdentifier;
						break;
					}

				case 1:
					{
						retVal = _massCreateObjectsTempTableRows[_rowNumber - 1].ObjectName;
						break;
					}

				case 2:
					{
						retVal = (object)_massCreateObjectsTempTableRows[_rowNumber - 1].ObjectArtifactID;
						break;
					}

				case 3:
					{
						retVal = (object)_massCreateObjectsTempTableRows[_rowNumber - 1].ObjectTypeID;
						break;
					}

				case 4:
					{
						retVal = (object)_massCreateObjectsTempTableRows[_rowNumber - 1].FieldID;
						break;
					}
			}

			return retVal;
		}
	}
}