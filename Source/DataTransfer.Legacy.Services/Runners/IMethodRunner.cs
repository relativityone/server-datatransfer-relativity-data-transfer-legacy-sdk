using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.Runners
{
	public interface IMethodRunner
	{
		Task<T> ExecuteAsync<T>(Func<Task<T>> func, int? workspaceId, string correlationId, [CallerMemberName] string callerMemberName = "");
	}
}