using System;
using System.Text;

namespace Relativity.MassImport.Data.SqlFramework
{
	internal static class ISqlQueryPartExtensions
	{
		public static string BuildQuery(this ISqlQueryPart query)
		{
			query = query ?? throw new ArgumentNullException(nameof(query));

			var sb = new StringBuilder();
			query.WriteTo(sb);
			return sb.ToString();
		}
	}
}
