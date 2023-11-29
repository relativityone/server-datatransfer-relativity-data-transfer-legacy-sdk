using System;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	[Serializable]
	public class DataSetWrapper : ISerializable
	{
		private const string FieldName = "DataSetXml";

		private readonly string _dataSetXml;
		private readonly DataSet _dataSet;

		public DataSetWrapper(DataSet dataSet)
		{
			_dataSet = dataSet;

			StringBuilder builder = new StringBuilder();
			using (TextWriter textWriter = new StringWriter(builder))
			{
				dataSet.WriteXml(textWriter, XmlWriteMode.WriteSchema);
			}

			_dataSetXml = builder.ToString();
		}

		public DataSetWrapper(SerializationInfo info, StreamingContext context)
		{
			_dataSetXml = (string) info.GetValue(FieldName, typeof(string));

			DataSet dataSet = new DataSet();
			using (TextReader reader = new StringReader(_dataSetXml))
			{
				dataSet.ReadXml(reader, XmlReadMode.ReadSchema);
			}

			_dataSet = dataSet;
		}

		public DataSet Unwrap()
		{
			return _dataSet;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(FieldName, _dataSetXml, typeof(string));
		}
	}
}