using Relativity.MassImport.Data.Cache;

namespace Relativity.MassImport.Core.Pipeline.Stages.Shared
{
	internal class LoadColumnDefinitionCacheStage<T> : Pipeline.Framework.IPipelineStage<T> where T : Pipeline.Input.Interface.IColumnDefinitionCacheInput, Pipeline.Input.Interface.IImportSettingsInput<NativeLoadInfo>
	{
		private readonly Pipeline.MassImportContext _context;

		public LoadColumnDefinitionCacheStage(Pipeline.MassImportContext context)
		{
			_context = context;
		}

		public T Execute(T input)
		{
			if (input.ColumnDefinitionCache is null)
			{
				input.ColumnDefinitionCache = this.LoadColumnDefinitionCache(input.Settings.RunID, input.Settings);
			}

			return input;
		}

		private ColumnDefinitionCache LoadColumnDefinitionCache(string runId, NativeLoadInfo settings)
		{
			var columnDefinitionCache = new ColumnDefinitionCache(_context.BaseContext.DBContext);
			columnDefinitionCache.LoadDataFromCache(runId);
			columnDefinitionCache.ValidateFieldMapping(settings.MappedFields);
			return columnDefinitionCache;
		}
	}
}