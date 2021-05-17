using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Telemetry.APM;

namespace Relativity.DataTransfer.Legacy.Services.Runners
{
	public sealed class MethodRunnerWithInstrumentation : IMethodRunner
	{
		private readonly IMethodRunner _methodRunner;
		private readonly IAPILog _logger;
		private readonly IAPM _apm;

		public MethodRunnerWithInstrumentation(IMethodRunner methodRunner, IAPILog logger, IAPM apm)
		{
			_methodRunner = methodRunner;
			_logger = logger;
			_apm = apm;
		}

		public async Task<T> ExecuteAsync<T>(Func<Task<T>> func, int? workspaceId, string correlationId, [CallerMemberName] string callerMemberName = "")
		{
			using (_logger.LogContextPushProperty("method-name", callerMemberName))
			using (_logger.LogContextPushProperty("workspace-id", workspaceId))
			using (_logger.LogContextPushProperty("correlation-id", correlationId))
			{
				var stopwatch = Stopwatch.StartNew();
				Exception exception = null;
				T result;
				try
				{
					result = await _methodRunner.ExecuteAsync(func, workspaceId, correlationId).ConfigureAwait(false);
				}
				catch (Exception e)
				{
					exception = e;
					throw;
				}
				finally
				{
					stopwatch.Stop();
					var data = new Dictionary<string, object>();
					if (exception != null)
					{
						data.Add("exception", exception);
						data.Add("status", "failed");
					}
					else
					{
						data.Add("status", "success");
					}

					_apm.TimedOperation("todo", stopwatch.ElapsedMilliseconds, Guid.Empty, correlationId, data);
				}

				return result;
			}
		}
	}
}