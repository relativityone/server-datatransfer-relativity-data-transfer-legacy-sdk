using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Relativity.MassImport.Data.SqlFramework
{
	internal class SerialSqlQuery : ISqlQueryPart
	{
		private IEnumerable<ISqlQueryPart> parts;

		public SerialSqlQuery(params ISqlQueryPart[] parts)
		{
			this.parts = parts;
		}

		public SerialSqlQuery(IEnumerable<ISqlQueryPart> parts)
		{
			this.parts = parts;
		}

		public void WriteTo(StringBuilder queryBuilder)
		{
			foreach (ISqlQueryPart sqlQueryContent in parts)
				sqlQueryContent?.WriteTo(queryBuilder);
		}

		public SerialSqlQuery Add(params ISqlQueryPart[] parts)
		{
			this.parts = this.parts.Concat(parts);
			return this;
		}

		public override string ToString()
		{
			var queryBuilder = new StringBuilder(256);
			WriteTo(queryBuilder);
			return queryBuilder.ToString();
		}
	}
}