using Relativity.MassImport.Data.StagingTables;

namespace Relativity.MassImport.Core.Pipeline.Stages.Job
{
	internal class CreateStagingTablesStage<T> : Pipeline.Framework.IPipelineStage<T> where T : Pipeline.Input.Interface.IExtractedTextInput, Pipeline.Input.Interface.IImportSettingsInput<Relativity.MassImport.DTO.NativeLoadInfo>, Pipeline.Input.Interface.IColumnDefinitionCacheInput
	{
		private readonly IStagingTableRepository _stagingTableRepository;

		public CreateStagingTablesStage(IStagingTableRepository stagingTableRepository)
		{
			_stagingTableRepository = stagingTableRepository;
		}

		public T Execute(T input)
		{
			var settings = input.Settings;

			bool excludeFolderPathForOldClient = settings.RootFolderID == 0;
			_stagingTableRepository.CreateStagingTables(input.ColumnDefinitionCache, settings, input.IncludeExtractedTextEncoding, excludeFolderPathForOldClient);

			return input;
		}
	}
}