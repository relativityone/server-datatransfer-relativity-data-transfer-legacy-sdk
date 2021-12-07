using Relativity.MassImport.Core.Pipeline.Framework;
using Relativity.MassImport.Core.Pipeline.Framework.Stages;
using Relativity.MassImport.Core.Pipeline.Input.Interface;
using Relativity.MassImport.Data.StagingTables;

namespace Relativity.MassImport.Core.Pipeline.Stages.Job
{
	internal class ExecuteIfJobNotInitializedStage<TInput> : ConditionalStage<TInput> where TInput : IImportSettingsInput<NativeLoadInfo>
	{
		private readonly IStagingTableRepository _stagingTableRepository;

		public ExecuteIfJobNotInitializedStage(
			IPipelineExecutor pipelineExecutor,
			IPipelineStage<TInput> innerStage,
			IStagingTableRepository stagingTableRepository) : base(pipelineExecutor, innerStage)
		{
			_stagingTableRepository = stagingTableRepository;
		}

		protected override bool ShouldExecute(TInput input)
		{
			bool areStagingTablesCreated = _stagingTableRepository.StagingTablesExist();

			// we need to initialize job only if staging tables doesn't exist yet
			return !areStagingTablesCreated;
		}
	}
}