using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Relativity.MassImport.Core.Pipeline.Framework.Stages
{
	internal sealed class CombinedStage
	{
		private CombinedStage()
		{
		}

		public static Framework.IPipelineStage<TInput, TOutput> Create<TInput, TIntermediate, TOutput>(Framework.IPipelineExecutor pipelineExecutor, Framework.IPipelineStage<TInput, TIntermediate> first, Framework.IPipelineStage<TIntermediate, TOutput> second)
		{
			return new CombinedStageType<TInput, TIntermediate, TOutput>(pipelineExecutor, first, second);
		}

		public static Framework.IPipelineStage<T> Create<T>(Framework.IPipelineExecutor pipelineExecutor, Framework.IPipelineStage<T> first, Framework.IPipelineStage<T> second)
		{
			CombinedStageType<T> firstAsCombinedStage = first as CombinedStageType<T>;

			if (firstAsCombinedStage is null)
			{
				return new CombinedStageType<T>(pipelineExecutor, first, second);
			}

			return firstAsCombinedStage.Add(second);
		}

		private class CombinedStageType<TInput, T, TOutput> : Framework.IPipelineStage<TInput, TOutput>, Stages.IInfrastructureStage
		{
			private readonly Framework.IPipelineExecutor pipelineExecutor;
			private readonly Framework.IPipelineStage<TInput, T> firstStage;
			private readonly Framework.IPipelineStage<T, TOutput> secondStage;

			public CombinedStageType(Framework.IPipelineExecutor pipelineExecutor, Framework.IPipelineStage<TInput, T> firstStage, Framework.IPipelineStage<T, TOutput> secondStage)
			{
				this.pipelineExecutor = pipelineExecutor;
				this.firstStage = firstStage;
				this.secondStage = secondStage;
			}

			public TOutput Execute(TInput input)
			{
				var x = pipelineExecutor.Execute(firstStage, input);
				return pipelineExecutor.Execute(secondStage, x);
			}

			public override string ToString()
			{
				var firstStageJson = JToken.Parse(firstStage.GetJson());
				var secondStageJson = JToken.Parse(secondStage.GetJson());
				var json = new JObject() { { "FirstStage", firstStageJson }, { "SecondStage", secondStageJson } };
				return json.ToString();
			}
		}

		private class CombinedStageType<T> : Framework.IPipelineStage<T>, Stages.IInfrastructureStage
		{
			private readonly Framework.IPipelineExecutor pipelineExecutor;
			private readonly List<Framework.IPipelineStage<T>> stages;

			public CombinedStageType(Framework.IPipelineExecutor pipelineExecutor, Framework.IPipelineStage<T> first, Framework.IPipelineStage<T> second)
			{
				this.pipelineExecutor = pipelineExecutor;
				stages = new List<Framework.IPipelineStage<T>>() { first, second };
			}

			public T Execute(T input)
			{
				var currentValue = input;
				foreach (Framework.IPipelineStage<T> stage in stages)
				{
					currentValue = pipelineExecutor.Execute(stage, currentValue);
				}

				return currentValue;
			}

			internal CombinedStageType<T> Add(Framework.IPipelineStage<T> stage)
			{
				stages.Add(stage);
				return this;
			}

			public override string ToString()
			{
				var stagesJson = new JArray();
				foreach (var stageJsonString in stages.Select(stage => stage.GetJson()))
				{
					var stageJson = JToken.Parse(stageJsonString);
					stagesJson.Add(stageJson);
				}

				var json = new JObject() { { "Stages", stagesJson } };
				return json.ToString();
			}
		}
	}
}