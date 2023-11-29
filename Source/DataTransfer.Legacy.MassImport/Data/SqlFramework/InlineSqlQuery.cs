using System;

namespace Relativity.MassImport.Data.SqlFramework
{
	internal class InlineSqlQuery : SqlQueryPart
	{
		public InlineSqlQuery(FormattableString sql)
		{
			SqlString = sql;
		}

		public override FormattableString SqlString { get; }
	}
}