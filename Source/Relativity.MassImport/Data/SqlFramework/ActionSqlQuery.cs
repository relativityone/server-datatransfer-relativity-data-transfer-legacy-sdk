using System;
using System.Text;

namespace Relativity.MassImport.Data.SqlFramework
{
	internal class ActionSqlQuery : ISqlQueryPart
	{
		private Action<StringBuilder> action;

		public ActionSqlQuery(Action<StringBuilder> action)
		{
			this.action = action;
		}

		public void WriteTo(StringBuilder queryBuilder)
		{
			action(queryBuilder);
		}

		public override string ToString()
		{
			var queryBuilder = new StringBuilder(256);
			WriteTo(queryBuilder);
			return queryBuilder.ToString();
		}
	}
}