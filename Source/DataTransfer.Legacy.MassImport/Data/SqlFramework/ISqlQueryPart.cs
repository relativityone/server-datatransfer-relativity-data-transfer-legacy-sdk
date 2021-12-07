using System.Text;

namespace Relativity.MassImport.Data.SqlFramework
{
	internal interface ISqlQueryPart
	{
		void WriteTo(StringBuilder queryBuilder);
	}
}