using Relativity.MassImport.Data.Cache;

namespace Relativity.MassImport.Core.Pipeline.Stages.Job
{
	internal class PopulateCacheStage<T> : Pipeline.Framework.IPipelineStage<T> where T : Pipeline.Input.Interface.IImportSettingsInput<Relativity.MassImport.DTO.NativeLoadInfo>
	{
		private readonly Pipeline.MassImportContext _context;

		public PopulateCacheStage(Pipeline.MassImportContext context)
		{
			_context = context;
		}

		public T Execute(T input)
		{
			var columnDefinitionCache = new ColumnDefinitionCache(_context.BaseContext.DBContext);
			columnDefinitionCache.InitializeCache(input.Settings.MappedFields, input.Settings.RunID);
			return input;
		}
	}
}