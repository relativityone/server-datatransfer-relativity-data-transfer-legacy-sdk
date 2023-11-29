using System;
using System.Text;

namespace Relativity.MassImport.Data.SqlFramework
{
	internal abstract class SqlQueryPart : ISqlQueryPart
	{
		public abstract FormattableString SqlString { get; }

		public virtual void WriteTo(StringBuilder queryBuilder)
		{
			var sql = SqlString;
			queryBuilder.AppendFormat(sql.Format, sql.GetArguments());
		}

		public override string ToString()
		{
			var queryBuilder = new StringBuilder(256);
			WriteTo(queryBuilder);
			return queryBuilder.ToString();
		}
	}
}