using System;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.Runners
{
	public class MethodRunnerWithToggleCheck : IMethodRunner
	{
		private readonly IMethodRunner _methodRunner;

		public MethodRunnerWithToggleCheck(IMethodRunner methodRunner)
		{
			_methodRunner = methodRunner;
		}

		public async Task<T> ExecuteAsync<T>(Func<Task<T>> func, int? workspaceId, string correlationId, string callerMemberName = "")
		{
			//TODO check for toggle value
			return await _methodRunner.ExecuteAsync(func, workspaceId, correlationId).ConfigureAwait(false);
		}
	}
}