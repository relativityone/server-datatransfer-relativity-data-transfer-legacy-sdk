using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;

namespace MassImport.NUnit.Integration.Helpers
{
	public static class Validator
	{
		public static void ThenTheFieldsHaveCorrectValues(
			TestWorkspace testWorkspace, 
			DataTable expected, 
			string identifierFieldName = WellKnownFields.ControlNumber)
		{
			var objectTypeName = !string.IsNullOrEmpty(expected.TableName) ? expected.TableName : "Document";
			List<string> fieldNames = expected.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList();
			DataTable actual = GetActualFieldValues(testWorkspace, fieldNames, objectTypeName);

			foreach (DataRow expectedRow in expected.Rows.Cast<DataRow>())
			{
				string identifierValue = expectedRow[identifierFieldName].ToString();
				DataRow actualRow = actual.Rows.Cast<DataRow>()
					.FirstOrDefault(row => row[identifierFieldName].ToString() == identifierValue);
				Assert.IsNotNull(actualRow, $"Could not find {objectTypeName} with identifier: {identifierValue}");
				ThenTheRowHasCorrectValues(fieldNames, expectedRow, actualRow);
			}
		}

		public static void ThenTheFoldersHaveCorrectValues(TestWorkspace testWorkspace, string[] expectedFolders)
		{
			string query = @" SELECT [Name] FROM [Folder] WHERE [ArtifactID] != 1003697";
			using (SqlConnection connection = new SqlConnection(testWorkspace.EddsConnectionString))
			using (SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection))
			{
				DataTable result = new DataTable();
				dataAdapter.Fill(result);

				Assert.That(result.Rows.Count, Is.EqualTo(expectedFolders.Length));

				for (var i = 0; i < result.Rows.Count; i++)
				{
					var row = result.Rows[i];
					var actualValue = row[0].ToString();
					Assert.That(actualValue, Is.EqualTo(expectedFolders[i]));
				}
			}
		}

		public static void ThenTableIsNotCreated(TestWorkspace testWorkspace, string tableName)
		{
			string query = $"SELECT * FROM [EDDS{testWorkspace.WorkspaceId}].[EDDSDBO].[{tableName}]";
			string exceptionMessage = "";
			try
			{
				DataTable result = new DataTable();

				using (SqlConnection connection = new SqlConnection(testWorkspace.EddsConnectionString))
				using (SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection))
				{
					dataAdapter.Fill(result);
				}
			}
			catch (Exception ex)
			{
				exceptionMessage = ex.Message;
			}
			finally
			{
				Assert.AreEqual(exceptionMessage,
					$"Invalid object name 'EDDS{testWorkspace.WorkspaceId}.EDDSDBO.{tableName}'.");
			}

		}

		public static void ThenImportStatusAndErrorDataIsSetInNativeTempTable(TestWorkspace testWorkspace, string runId, string[] expectedResult)
		{
			string query = $"SELECT [ControlNumber], [kCura_Import_Status], [kCura_Import_ErrorData] FROM [EDDS{testWorkspace.WorkspaceId}].[Resource].[RELNATTMP_{runId}]";
			using (SqlConnection connection = new SqlConnection(testWorkspace.EddsConnectionString))
			using (SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection))
			{
				DataTable result = new DataTable();
				dataAdapter.Fill(result);

				Assert.That(result.Rows.Count, Is.EqualTo(expectedResult.Length));

				for (var i = 0; i < result.Rows.Count; i++)
				{
					var row = result.Rows[i];
					var actualValue = $"{row[0]}||{row[1]}||{row[2]}";
					Assert.That(actualValue, Is.EqualTo(expectedResult[i]));
				}
			}
		}

		private static DataTable GetActualFieldValues(TestWorkspace testWorkspace, List<string> fieldNames, string objectTypeName)
		{
			string query = BuildDocumentQuery(fieldNames, objectTypeName);
			using (SqlConnection connection = new SqlConnection(testWorkspace.EddsConnectionString))
			using (SqlDataAdapter dataAdapter = new SqlDataAdapter(query, connection))
			{
				DataTable result = new DataTable();
				dataAdapter.Fill(result);

				return result;
			}
		}

		private static string BuildDocumentQuery(List<string> fieldNames, string objectTypeName)
		{
			string columns = string.Join(", ", fieldNames.Select(fieldName => $"[{fieldName}]"));
			return $"SELECT {columns} FROM [{objectTypeName}]";
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