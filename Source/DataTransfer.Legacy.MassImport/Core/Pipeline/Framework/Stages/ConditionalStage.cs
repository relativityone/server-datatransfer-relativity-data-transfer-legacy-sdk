using Newtonsoft.Json.Linq;

namespace Relativity.MassImport.Core.Pipeline.Framework.Stages
{
	internal abstract class ConditionalStage<TInput> : Framework.IPipelineStage<TInput>
	{
		private readonly Framework.IPipelineExecutor _pipelineExecutor;
		private readonly Framework.IPipelineStage<TInput> _innerStage;

		public ConditionalStage(Framework.IPipelineExecutor pipelineExecutor, Framework.IPipelineStage<TInput> innerStage)
		{
			_pipelineExecutor = pipelineExecutor;
			_innerStage = innerStage;
		}

		public TInput Execute(TInput input)
		{
			if (ShouldExecute(input))
			{
				return _pipelineExecutor.Execute(_innerStage, input);
			}

			return input;
		}

		protected abstract bool ShouldExecute(TInput input);

		public override string ToString()
		{
			string conditionName = this.GetUserFriendlyStageName();
			var innerStageJson = JToken.Parse(_innerStage.GetJson());
			var json = new JObject() { { conditionName, innerStageJson } };
			return json.ToString();
		}
	}
}