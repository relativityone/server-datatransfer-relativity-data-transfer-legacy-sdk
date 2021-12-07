using System.Collections.Generic;

namespace Relativity.MassImport.Data.DataGridWriteStrategy
{
	// TODO: change to internal and adjust namespace, https://jira.kcura.com/browse/REL-477112 
	public interface IDataGridSqlTempReader
	{
		IEnumerable<string> GetAllIdentifiersFromSql(string tempTableName, string identifierName);
		string GetFieldFromSqlAsString(string tempTableName, string identifierName, string identifier, string fieldName);
	}
}