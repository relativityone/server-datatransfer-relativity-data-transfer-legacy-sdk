using System.Text;

namespace Relativity.MassImport.Data.SqlFramework
{
	internal class PrintSectionQuery : ISqlQueryPart
	{
		public static string MASS_IMPORT_SECTION = "Mass Import Section: ";
		private ISqlQueryPart query;
		private string name;

		public PrintSectionQuery(ISqlQueryPart query, string name)
		{
			this.query = query;
			this.name = name;
		}

		public void WriteTo(StringBuilder queryBuilder)
		{
			queryBuilder.AppendLine();
			queryBuilder.AppendLine($"PRINT '{MASS_IMPORT_SECTION}{name}';");
			query.WriteTo(queryBuilder);
		}
	}
}