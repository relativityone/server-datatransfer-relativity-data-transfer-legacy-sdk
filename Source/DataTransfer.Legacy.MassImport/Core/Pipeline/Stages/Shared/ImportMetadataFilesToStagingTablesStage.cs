using Relativity.MassImport.Data.StagingTables;

namespace Relativity.MassImport.Core.Pipeline.Stages.Shared
{
	internal class ImportMetadataFilesToStagingTablesStage<T> : Pipeline.Framework.IPipelineStage<T> where T : Pipeline.Input.Interface.IImportSettingsInput<Relativity.MassImport.DTO.NativeLoadInfo>, Pipeline.Input.Interface.IColumnDefinitionCacheInput
	{
		private readonly Pipeline.MassImportContext _context;
		private readonly IStagingTableRepository _stagingTableRepository;

		public ImportMetadataFilesToStagingTablesStage(Pipeline.MassImportContext context, IStagingTableRepository stagingTableRepository)
		{
			_context = context;
			_stagingTableRepository = stagingTableRepository;
		}

		public T Execute(T input)
		{
			var settings = input.Settings;
			bool excludeFolderPathForOldClient = settings.RootFolderID == 0;
			if (settings.UseBulkDataImport)
			{
				if (string.IsNullOrEmpty(settings.BulkLoadFileFieldDelimiter))
				{
					settings.BulkLoadFileFieldDelimiter = Relativity.Data.Config.BulkLoadFileFieldDelimiter;
				}

				var bulkFileSharePath = string.IsNullOrEmpty(settings.BulkFileSharePath) ?
					_context.BaseContext.GetBcpSharePath() :
					settings.BulkFileSharePath;

				_stagingTableRepository.BulkInsert(settings, bulkFileSharePath, _context.Logger);
			}
			else
			{
				settings.RunID = _stagingTableRepository.Insert(input.ColumnDefinitionCache, settings, excludeFolderPathForOldClient);
			}

			return input;
		}
	}
}