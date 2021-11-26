using Relativity.MassImport.Data.StagingTables;

namespace Relativity.MassImport.Core.Pipeline.Stages.Job
{
	internal class TruncateStagingTablesStage<T> : Pipeline.Framework.IPipelineStage<T> where T : Pipeline.Input.Interface.IExtractedTextInput, Pipeline.Input.Interface.IImportSettingsInput<NativeLoadInfo>, Pipeline.Input.Interface.IColumnDefinitionCacheInput
	{
		private readonly IStagingTableRepository _stagingTableRepository;

		public TruncateStagingTablesStage(IStagingTableRepository stagingTableRepository)
		{
			_stagingTableRepository = stagingTableRepository;
		}

		public T Execute(T input)
		{
			var settings = input.Settings;
			_stagingTableRepository.TruncateStagingTables(settings.MappedFields, settings.LoadImportedFullTextFromServer);

			return input;
		}
	}
}