
namespace Relativity.MassImport.Core.Pipeline.Framework
{
	internal interface IPipelineStage<TInput, TOutput>
	{
		TOutput Execute(TInput input);
	}

	internal interface IPipelineStage<TInput> : IPipelineStage<TInput, TInput>
	{
	}
}