using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.Runners
{
	public sealed class MethodRunner : IMethodRunner
	{
		public async Task<T> ExecuteAsync<T>(Func<Task<T>> func, int? workspaceId, string correlationId, [CallerMemberName] string callerMemberName = "")
		{
			return await func().ConfigureAwait(false);
		}
	}
}