using Relativity.MassImport.Data;

namespace Relativity.MassImport.Core.Pipeline.Framework
{
	internal class PipelineExecutorImportMeasurementsDecorator : Framework.IPipelineExecutor
	{
		private readonly Framework.IPipelineExecutor decoratedObject;
		private readonly ImportMeasurements _importMeasurements;

		public PipelineExecutorImportMeasurementsDecorator(Framework.IPipelineExecutor decoratedObject, ImportMeasurements importMeasurements)
		{
			this.decoratedObject = decoratedObject;
			_importMeasurements = importMeasurements;
		}

		public TOutput Execute<TInput, TOutput>(Framework.IPipelineStage<TInput, TOutput> pipelineStage, TInput input)
		{
			if (pipelineStage is Framework.Stages.IInfrastructureStage)
			{
				return decoratedObject.Execute(pipelineStage, input);
			}

			string stageName = pipelineStage.GetType().Name;
			TOutput result;
			try
			{
				_importMeasurements.StartMeasure(stageName);
				result = decoratedObject.Execute(pipelineStage, input);
			}
			finally
			{
				_importMeasurements.StopMeasure(stageName);
			}

			return result;
		}
	}
}