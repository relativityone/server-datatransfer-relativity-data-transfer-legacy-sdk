using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using Relativity.Services.Exceptions;

namespace Relativity.DataTransfer.Legacy.Services.Runners
{
	public sealed class MethodRunnerWithErrorHandling : IMethodRunner
	{
		private readonly IMethodRunner _methodRunner;

		public MethodRunnerWithErrorHandling(IMethodRunner methodRunner)
		{
			_methodRunner = methodRunner;
		}

		public async Task<T> ExecuteAsync<T>(Func<Task<T>> func, int? workspaceId, string correlationId, [CallerMemberName] string callerMemberName = "")
        {
            try
            {
                return await _methodRunner.ExecuteAsync(func, workspaceId, correlationId).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                var msg = $"ERROR1: {e}, ERROR2: {e.InnerException}";
                throw new Exception(msg);
            }
        }
	}
}