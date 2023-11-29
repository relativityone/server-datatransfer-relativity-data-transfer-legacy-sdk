using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Relativity;

namespace MassImport.NUnit.Integration.Helpers
{
	public static class RandomHelper
	{
		private static readonly Random Random = new Random();

		private static readonly Dictionary<FieldTypeHelper.FieldType, Func<FieldInfo, object>> FieldValueProvider =
			new Dictionary<FieldTypeHelper.FieldType, Func<FieldInfo, object>>
			{
				{ FieldTypeHelper.FieldType.Varchar, (fieldInfo) => NextString(10, fieldInfo.TextLength) },
				{ FieldTypeHelper.FieldType.Boolean, (_) => NextBool() ? "1" : "0" },
			};

		public static string NextString(int minLength, int maxLength)
		{
			const string availableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			int length = Random.Next(minLength, maxLength);

			return new string(Enumerable.Repeat(availableChars, length)
				.Select(s => s[Random.Next(s.Length)]).ToArray());
		}

		public static bool NextBool()
		{
			return Random.NextDouble() > 0.5;
		}

		public static DataTable GetFieldValues(FieldInfo[] fields, int numberOfArtifactsToCreate)
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