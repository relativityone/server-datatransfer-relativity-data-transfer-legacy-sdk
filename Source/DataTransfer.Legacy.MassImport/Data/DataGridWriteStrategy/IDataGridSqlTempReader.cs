using System.Collections.Generic;

namespace Relativity.MassImport.Data.DataGridWriteStrategy
{
	internal interface IDataGridSqlTempReader
	{
		IEnumerable<string> GetAllIdentifiersFromSql(string tempTableName, string identifierName);
		string GetFieldFromSqlAsString(string tempTableName, string identifierName, string identifier, string fieldName);
	}
}