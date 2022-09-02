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

        private static DataTable GetActualFieldValues(TestWorkspace testWorkspace, List<string> fieldNames, string objectTypeName)
        {
            string query = BuildDocumentQuery(fieldNames, objectTypeName);
            using (SqlConnection connection = new SqlConnection(testWorkspace.ConnectionString))
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