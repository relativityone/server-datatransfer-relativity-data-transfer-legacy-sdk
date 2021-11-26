using Relativity.MassImport.Data.Cache;

namespace Relativity.MassImport.Core.Pipeline.Input.Interface
{
	internal interface IColumnDefinitionCacheInput
	{
		ColumnDefinitionCache ColumnDefinitionCache { get; set; }
	}
}