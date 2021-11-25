using System;
using System.Text;

namespace Relativity.MassImport.Data.SqlFramework
{
	internal class IfQuery : ISqlQueryPart
	{
		private readonly ISqlQueryPart _condition;
		private readonly ISqlQueryPart _truePart;
		private readonly ISqlQueryPart _falsePart;

		public IfQuery(ISqlQueryPart condition, ISqlQueryPart truePart, ISqlQueryPart falsePart)
		{
			_condition = condition ?? throw new ArgumentNullException(nameof(condition));
			_truePart = truePart ?? throw new ArgumentNullException(nameof(truePart));
			_falsePart = falsePart ?? throw new ArgumentNullException(nameof(falsePart));
		}

		public void WriteTo(StringBuilder queryBuilder)
		{
			queryBuilder.AppendLine($"IF {_condition}");
			WritePart(queryBuilder, _truePart);
			queryBuilder.AppendLine(@"ELSE");
			WritePart(queryBuilder, _falsePart);
		}

		private void WritePart(StringBuilder queryBuilder, ISqlQueryPart part)
		{
			queryBuilder.AppendLine("BEGIN");
			part.WriteTo(queryBuilder);
			queryBuilder.AppendLine();
			queryBuilder.AppendLine(@"END");
		}
	}
}
