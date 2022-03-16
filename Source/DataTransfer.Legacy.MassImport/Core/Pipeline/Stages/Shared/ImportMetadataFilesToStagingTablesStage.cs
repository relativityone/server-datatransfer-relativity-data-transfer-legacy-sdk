﻿using Relativity.MassImport.Data.StagingTables;

namespace Relativity.MassImport.Core.Pipeline.Stages.Shared
{
	using Castle.Core.Internal;

	internal class ImportMetadataFilesToStagingTablesStage<T> : Pipeline.Framework.IPipelineStage<T> where T : Pipeline.Input.Interface.IImportSettingsInput<NativeLoadInfo>, Pipeline.Input.Interface.IColumnDefinitionCacheInput
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

				var bcp = _context.BulkFileSharePath.IsNullOrEmpty() ?
					_context.BaseContext.GetBcpSharePath() :
					_context.BulkFileSharePath;

				_stagingTableRepository.BulkInsert(settings, bcp);
			}
			else
			{
				settings.RunID = _stagingTableRepository.Insert(input.ColumnDefinitionCache, settings, excludeFolderPathForOldClient);
			}

			return input;
		}
	}
}