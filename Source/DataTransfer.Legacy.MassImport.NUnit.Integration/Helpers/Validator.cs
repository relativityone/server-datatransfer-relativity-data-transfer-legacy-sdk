using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;

namespace MassImport.NUnit.Integration.Helpers
{
	public static class Validator
	{
		public static void ThenTheFieldsHaveCorrectValues(TestWorkspace testWorkspace, DataTable expected)
		{
			List<string> fieldNames = expected.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList();
			DataTable actual = GetActualFieldValues(testWorkspace, fieldNames);

			foreach (DataRow expectedRow in expected.Rows.Cast<DataRow>())
			{
				string controlNumber = expectedRow[WellKnownFields.ControlNumber].ToString();
				DataRow actualRow = actual.Rows.Cast<DataRow>()
					.FirstOrDefault(row => row[WellKnownFields.ControlNumber].ToString() == controlNumber);
				Assert.IsNotNull(actualRow, $"Could not find document with control number: {controlNumber}");
				ThenTheRowHasCorrectValues(fieldNames, expectedRow, actualRow);
			}
		}

		private static DataTable GetActualFieldValues(TestWorkspace testWorkspace, List<string> fieldNames)
		{
			string query = BuildDocumentQuery(fieldNames);
			using (SqlConnection connection = new SqlConnection(testWorkspace.ConnectionString))
			using (SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection))
			{
				DataTable result = new DataTable();
				dataAdapter.Fill(result);

				return result;
			}
		}

		private static string BuildDocumentQuery(List<string> fieldNames)
		{
			string columns = string.Join(", ", fieldNames);
			return $"SELECT {columns} FROM [Document]";
		}

		private static void ThenTheRowHasCorrectValues(List<string> columnNames, DataRow expected, DataRow actual)
		{
			foreach (string columnName in columnNames)
			{
				string expectedValue = expected[columnName].ToString();
				string actualValue = actual[columnName].ToString();
				if (actual[columnName] is bool value)
				{
					actualValue = value ? "1" : "0";
				}

				Assert.AreEqual(expectedValue, actualValue, $"Incorrect value in {columnName} field");
			}
		}
	}
}