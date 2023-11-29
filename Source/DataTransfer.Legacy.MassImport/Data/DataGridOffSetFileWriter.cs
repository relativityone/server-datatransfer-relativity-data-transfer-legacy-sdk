using System.Collections.Generic;
using System.IO;

namespace Relativity.MassImport.Data
{
	internal class DataGridOffSetFileWriter : IDataGridOffSetWriter
	{
		private string OffSetFilePath;
		private string offSetHeader;
		private List<DataGridOffSetInfo> offSetInfoList;

		public DataGridOffSetFileWriter(string fileName, string headerStr)
		{
			OffSetFilePath = fileName;
			offSetHeader = headerStr;
			offSetInfoList = new List<DataGridOffSetInfo>();
		}

		public void AddOffSetRecord(DataGridOffSetInfo offSetInfo)
		{
			offSetInfoList.Add(offSetInfo);
		}

		public void Flush()
		{
			using (var fileWriterStream = new StreamWriter(OffSetFilePath, true))
			{
				fileWriterStream.WriteLine(offSetHeader);
				foreach (DataGridOffSetInfo offSetInfo in offSetInfoList)
				{
					string fieldOffetStr = "";
					foreach (DataGridFieldOffSetInfo fieldInfo in offSetInfo.DataGridOffsetFieldInfo)
					{
						if (ReferenceEquals(fieldOffetStr, string.Empty))
						{
							fieldOffetStr = $"{fieldInfo.StartPosition}-{fieldInfo.Length}";
						}
						else
						{
							fieldOffetStr = $"{fieldOffetStr};{fieldInfo.StartPosition}-{fieldInfo.Length}";
						}
					}

					fileWriterStream.WriteLine($"{offSetInfo.FieldIdentifier};{fieldOffetStr}");
				}
			}
		}
	}
}