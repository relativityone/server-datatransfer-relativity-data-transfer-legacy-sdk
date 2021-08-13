using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.Helpers
{
	public static class RandomHelper
	{
		private static readonly Random Random = new Random();

		private static readonly Dictionary<FieldType, Func<SDK.ImportExport.V1.Models.FieldInfo, object>> FieldValueProvider =
			new Dictionary<FieldType, Func<SDK.ImportExport.V1.Models.FieldInfo, object>>
			{
				{ FieldType.Varchar, (fieldInfo) => NextString(5, fieldInfo.TextLength) },
				{ FieldType.Boolean, (_) => NextBool() ? "1" : "0" },
			};

		private static string NextString(int minLength, int maxLength)
		{
			const string availableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			int length = Random.Next(minLength, maxLength);

			return new string(Enumerable.Repeat(availableChars, length)
				.Select(s => s[Random.Next(s.Length)]).ToArray());
		}

		private static bool NextBool()
		{
			return Random.NextDouble() > 0.5;
		}

		public static DataTable GetFieldValues(IEnumerable<SDK.ImportExport.V1.Models.FieldInfo> fields, int numberOfArtifactsToCreate)
		{
			DataTable fieldValues = new DataTable();
			DataColumn[] columns = fields.Select(field => new DataColumn(field.DisplayName.Replace(" ", ""))).ToArray();

			fieldValues.Columns.AddRange(columns);

			for (int i = 0; i < numberOfArtifactsToCreate; i++)
			{
				object[] values = fields.Select(field => FieldValueProvider[field.Type].Invoke(field)).ToArray();
				fieldValues.Rows.Add(values);
			}

			return fieldValues;
		}
	}
}