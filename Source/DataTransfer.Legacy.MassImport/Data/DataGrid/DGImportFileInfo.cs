using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.SqlServer.Server;

namespace Relativity.MassImport.Data.DataGrid
{
	internal class DGImportFileInfo
	{
		public int ImportId;
		public int FieldArtifactId;
		public string FieldName;
		public string FieldNamespace;
		public string FileLocation;
		public ulong FileSize;
		public string Checksum;
		public string IndexName;
        public bool LinkedText;
    }

	static class DGImportFileInfoEnumerable
	{
		public static IEnumerable<SqlDataRecord> GetDgImportFileInfoAsDataRecord(this IEnumerable<DGImportFileInfo> dgInfoPaths)
		{
			var sqlDataRecord = new SqlDataRecord(
				new SqlMetaData(nameof(DGImportFileInfo.ImportId), SqlDbType.Int), 
				new SqlMetaData(nameof(DGImportFileInfo.FieldArtifactId), SqlDbType.Int), 
				new SqlMetaData(nameof(DGImportFileInfo.FileLocation), SqlDbType.NVarChar, 2000L), 
				new SqlMetaData(nameof(DGImportFileInfo.FileSize), SqlDbType.BigInt), 
				new SqlMetaData(nameof(DGImportFileInfo.Checksum), SqlDbType.NVarChar, SqlMetaData.Max)
			);

			foreach (DGImportFileInfo dgImportFileInfo in dgInfoPaths)
			{
				sqlDataRecord.SetInt32(0, dgImportFileInfo.ImportId);
				sqlDataRecord.SetInt32(1, dgImportFileInfo.FieldArtifactId);
				if (dgImportFileInfo.FileLocation is null)
				{
					sqlDataRecord.SetDBNull(2);
				}
				else
				{
					sqlDataRecord.SetString(2, dgImportFileInfo.FileLocation);
				}

				sqlDataRecord.SetSqlInt64(3, Convert.ToInt64(dgImportFileInfo.FileSize));
				if (dgImportFileInfo.Checksum is null)
				{
					sqlDataRecord.SetDBNull(4);
				}
				else
				{
					sqlDataRecord.SetString(4, dgImportFileInfo.Checksum);
				}

				yield return sqlDataRecord;
			}
		}

        public static IDataReader GetDgImportFileInfoAsDataReader(this IEnumerable<DGImportFileInfo> dgInfoPaths)
        {
            var baseTable = new DataTable();
            baseTable.Columns.Add(new DataColumn(nameof(DGImportFileInfo.ImportId), typeof(int)));
            baseTable.Columns.Add(new DataColumn(nameof(DGImportFileInfo.FieldArtifactId), typeof(int)));
            baseTable.Columns.Add(new DataColumn(nameof(DGImportFileInfo.FileLocation), typeof(string)));
            baseTable.Columns.Add(new DataColumn(nameof(DGImportFileInfo.FileSize), typeof(long)));
            baseTable.Columns.Add(new DataColumn(nameof(DGImportFileInfo.Checksum), typeof(string)));
            baseTable.Columns.Add(new DataColumn(nameof(DGImportFileInfo.LinkedText), typeof(bool)));

            foreach (DGImportFileInfo dgInfoPath in dgInfoPaths)
            {
                baseTable.Rows.Add(dgInfoPath.ImportId, dgInfoPath.FieldArtifactId, dgInfoPath.FileLocation, dgInfoPath.FileSize, dgInfoPath.Checksum, dgInfoPath.LinkedText);
            }

            return baseTable.CreateDataReader();
        }
    }
}