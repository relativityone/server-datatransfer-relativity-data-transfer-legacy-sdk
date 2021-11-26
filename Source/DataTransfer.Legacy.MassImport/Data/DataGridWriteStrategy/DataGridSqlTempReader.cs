using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.Data;
using kCura.Data.RowDataGateway;

namespace Relativity.MassImport.Data.DataGridWriteStrategy
{
	internal class DataGridSqlTempReader : IDataGridSqlTempReader
	{
		private BaseContext _context;
		private const string _GET_FIELD_FROM_TEMP_TABLE_SQL_FORMAT = "SELECT ISNULL({0}, '') FROM [Resource].{1} WHERE {2} = @identifier";
		private const string _GET_ALL_IDENTIFIERS = "SELECT {0} FROM [Resource].{1}";

		public DataGridSqlTempReader(BaseContext context)
		{
			_context = context;
		}

		public string GetFieldFromSqlAsString(string tempTableName, string identifierName, string identifier, string fieldName)
		{
			var getValuesParams = new ParameterList();
			getValuesParams.Add(new SqlParameter("@identifier", SqlDbType.VarChar) { Value = identifier });
			
			string sql = string.Format(_GET_FIELD_FROM_TEMP_TABLE_SQL_FORMAT, kCura.Utility.SqlNameHelper.GetSqlObjectName(fieldName), kCura.Utility.SqlNameHelper.GetSqlObjectName(tempTableName), kCura.Utility.SqlNameHelper.GetSqlObjectName(identifierName));

			string fieldValue = _context.ExecuteSqlStatementAsScalar<string>(sql, getValuesParams);
			
			return fieldValue;
		}

		public IEnumerable<string> GetAllIdentifiersFromSql(string tempTableName, string identifierName)
		{
			string sql = string.Format(_GET_ALL_IDENTIFIERS, kCura.Utility.SqlNameHelper.GetSqlObjectName(identifierName), kCura.Utility.SqlNameHelper.GetSqlObjectName(tempTableName));
			
			List<string> identifiers = _context.ExecuteSqlStatementAsList(sql, reader => reader.GetString(0));
			
			return identifiers;
		}
	}
}