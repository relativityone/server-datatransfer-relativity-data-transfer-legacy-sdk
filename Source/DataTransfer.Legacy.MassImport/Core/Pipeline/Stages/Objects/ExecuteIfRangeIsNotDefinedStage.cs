
namespace Relativity.MassImport.Core.Pipeline.Stages.Objects
{
	internal class ExecuteIfRangeIsNotDefinedStage<TInput> : Pipeline.Framework.Stages.ConditionalStage<TInput> where TInput : Pipeline.Input.Interface.IImportSettingsInput<Relativity.MassImport.DTO.NativeLoadInfo>
	{
		public ExecuteIfRangeIsNotDefinedStage(Pipeline.Framework.IPipelineExecutor pipelineExecutor, Pipeline.Framework.IPipelineStage<TInput> innerStage) : base(pipelineExecutor, innerStage)
		{
		}

		protected override bool ShouldExecute(TInput input)
		{
			return input.Settings.Range is null;
		}
	}
}