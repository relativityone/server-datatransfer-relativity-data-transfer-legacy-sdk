using Newtonsoft.Json.Linq;

namespace Relativity.MassImport.Core.Pipeline.Framework.Stages
{
	internal abstract class DecoratorStage<TInput, TOutput> : IPipelineStage<TInput, TOutput>, ICustomJsonSerializationStage
	{
		private readonly IPipelineExecutor _pipelineExecutor;
		private readonly IPipelineStage<TInput, TOutput> _decoratedStage;

		protected DecoratorStage(IPipelineExecutor pipelineExecutor, IPipelineStage<TInput, TOutput> decoratedStage)
		{
			_pipelineExecutor = pipelineExecutor;
			_decoratedStage = decoratedStage;
		}

		public virtual TOutput Execute(TInput input)
		{
			return _pipelineExecutor.Execute(_decoratedStage, input);
		}

		public override string ToString()
		{
			var decoratorStageName = this.GetUserFriendlyStageName();
			var decoratedStageJson = JToken.Parse(_decoratedStage.GetJson());
			var json = new JObject
			{
				{
					decoratorStageName, decoratedStageJson
				}
			};
			return json.ToString();
		}
	}
}
