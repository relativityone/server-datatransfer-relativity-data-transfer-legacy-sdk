using System.Text;

namespace Relativity.MassImport.Data.SqlFramework
{
	internal class StatisticsTimeOnQuery : ISqlQueryPart
	{
		private ISqlQueryPart query;

		public StatisticsTimeOnQuery(ISqlQueryPart query)
		{
			this.query = query;
		}

		public void WriteTo(StringBuilder queryBuilder)
		{
			queryBuilder.AppendLine("SET STATISTICS TIME ON;");
			queryBuilder.AppendLine();
			query.WriteTo(queryBuilder);
			queryBuilder.AppendLine();
			queryBuilder.AppendLine("SET STATISTICS TIME OFF;");
		}
	}
}