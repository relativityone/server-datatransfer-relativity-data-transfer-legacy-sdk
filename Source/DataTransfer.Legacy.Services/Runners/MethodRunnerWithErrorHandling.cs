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
            catch (SoapException soapException)
            {
                //todo
                throw new ServiceException("todo", soapException);
            }
            catch (ServiceException serviceException)
            {
                //todo
                throw new ServiceException("todo", serviceException);
            }
            catch (Exception exception)
            {
                //todo
                throw new ServiceException("todo", exception);
            }
		}
	}
}