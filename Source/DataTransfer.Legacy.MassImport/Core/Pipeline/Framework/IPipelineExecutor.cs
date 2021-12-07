
namespace Relativity.MassImport.Core.Pipeline.Framework
{
	internal interface IPipelineExecutor
	{
		TOutput Execute<TInput, TOutput>(Framework.IPipelineStage<TInput, TOutput> pipelineStage, TInput input);
	}
}