namespace Relativity.MassImport.Data.Cache
{
	internal interface IColumnDefinitionCache
	{
		ColumnDefinitionInfo this[int artifactId] { get; }
	}
}