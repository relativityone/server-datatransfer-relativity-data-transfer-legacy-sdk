namespace Relativity.MassImport.Core.Pipeline.Framework
{
	internal class PipelineExecutor : Framework.IPipelineExecutor
	{
		public TOutput Execute<TInput, TOutput>(Framework.IPipelineStage<TInput, TOutput> pipelineStage, TInput input)
		{
			return pipelineStage.Execute(input);
		}
	}
}