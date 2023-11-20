using System.Collections.Generic;
using System.Text;

namespace DataTransfer.Legacy.MassImport.NUnit.Integration.Helpers
{
	internal static class ObjectHelper
	{
		public static string GetObjectsFile(string fieldDelimiter, IEnumerable<ObjectStagingTableRow> objectsRows)
		{
			StringBuilder metadataBuilder = new StringBuilder();
			foreach (var objectRow in objectsRows)
			{
				metadataBuilder.AppendLine($"{string.Join(fieldDelimiter, objectRow.GetValues())}{fieldDelimiter}");
			}

			return metadataBuilder.ToString();
		}

		public class ObjectStagingTableRow
		{
			public string PrimaryObjectName { get; set; }
			public string SecondaryObjectName { get; set; }
			public int ObjectTypeID { get; set; }
			public int FieldID { get; set; }

			public string[] GetValues()
			{
				int objectArtifactId = -1;

				return new string[]
				{
					PrimaryObjectName,
					SecondaryObjectName,
					objectArtifactId.ToString(),
					ObjectTypeID.ToString(),
					FieldID.ToString(),
				};
			}
		}
	}
}