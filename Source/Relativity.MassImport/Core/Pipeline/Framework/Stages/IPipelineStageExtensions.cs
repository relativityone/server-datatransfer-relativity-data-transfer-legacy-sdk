using System.Linq;

namespace Relativity.MassImport.Core.Pipeline.Framework.Stages
{
	static class IPipelineStageExtensions
	{
		internal static IPipelineStage<T> AddNextStage<T>(this IPipelineStage<T> first, IPipelineStage<T> second, IPipelineExecutor pipelineExecutor)
		{
			return CombinedStage.Create(pipelineExecutor, first, second);
		}

		internal static IPipelineStage<TInput, TOutput> AddNextStage<TInput, T, TOutput>(this IPipelineStage<TInput, T> first, IPipelineStage<T, TOutput> second, IPipelineExecutor pipelineExecutor)
		{
			return CombinedStage.Create(pipelineExecutor, first, second);
		}

		internal static string GetJson<T1, T2>(this IPipelineStage<T1, T2> stage)
		{
			if (stage is Stages.ICustomJsonSerializationStage)
			{
				return stage.ToString();
			}
			else
			{
				return $"\"{stage.GetUserFriendlyStageName()}\"";
			}
		}

		internal static string GetUserFriendlyStageName<T1, T2>(this IPipelineStage<T1, T2> stage)
		{
			return stage.GetType().Name.Split('`').First();
		}
	}
}