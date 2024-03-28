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