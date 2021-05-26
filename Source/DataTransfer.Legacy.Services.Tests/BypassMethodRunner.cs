using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.Services.Runners;

namespace Relativity.DataTransfer.Legacy.Services.Tests
{
	public class BypassMethodRunner : IMethodRunner
	{
		public async Task<T> ExecuteAsync<T>(Func<Task<T>> func, int? workspaceId, string correlationId, string callerMemberName = "")
		{
			return await func();
		}
	}
}