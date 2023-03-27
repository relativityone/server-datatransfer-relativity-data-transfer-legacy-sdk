using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	[Serializable]
	public class ExportDataWrapper : ISerializable
	{
		private const string FieldName = "ExportDataXml";

		private readonly string _xml;
		private readonly object[][] _data;

		public int? SerializedDataLength => _xml?.Length;

		public ExportDataWrapper(object[] exportData)
		{
			if (exportData == null)
			{
				_data = null;
				_xml = null;
			}
			else
			{
				_data = exportData.Cast<object[]>().ToArray();
				_xml = Serialize(_data);
			}
		}

		public ExportDataWrapper(object[][] exportData)
		{
			if (exportData == null)
			{
				_data = null;
				_xml = null;
			}
			else
			{
				_data = exportData;
				_xml = Serialize(_data);
			}
		}

		public ExportDataWrapper(SerializationInfo info, StreamingContext context)
		{
			_xml = (string) info.GetValue(FieldName, typeof(string));

			if (_xml == null)
			{
				_data = null;
			}
			else
			{
				XmlSerializer serializer = new XmlSerializer(typeof(object[][]));
				using (TextReader reader = new StringReader(_xml))
				{
					_data = (object[][]) serializer.Deserialize(reader);
				}
			}
		}

		public object[][] Unwrap()
		{
			return _data;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(FieldName, _xml, typeof(string));
		}

		private static string Serialize(object[][] data)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(object[][]));
			StringBuilder builder = new StringBuilder();
			using (TextWriter textWriter = new StringWriter(builder))
			{
				serializer.Serialize(textWriter, data);
			}

			return builder.ToString();
		}
	}
}