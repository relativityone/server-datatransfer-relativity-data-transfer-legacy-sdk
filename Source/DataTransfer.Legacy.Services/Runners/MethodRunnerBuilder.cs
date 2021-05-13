using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.Telemetry.APM;

namespace Relativity.DataTransfer.Legacy.Services.Runners
{
	public class MethodRunnerBuilder
	{
		private readonly IAPILog _logger;
		private readonly IAPM _apm;
		private readonly IServiceContextFactory _serviceContextFactory;

		public MethodRunnerBuilder(IAPILog logger, IAPM apm, IServiceContextFactory serviceContextFactory)
		{
			_logger = logger;
			_apm = apm;
			_serviceContextFactory = serviceContextFactory;
		}

		public IMethodRunner Build()
		{
			var methodRunner = new MethodRunner();
			var permissions = new MethodRunnerWithPermissionCheck(methodRunner, _serviceContextFactory);
			var errorHandling = new MethodRunnerWithErrorHandling(permissions);
			var toggle = new MethodRunnerWithToggleCheck(errorHandling);
			var instrumentation = new MethodRunnerWithInstrumentation(toggle, _logger, _apm);
			return instrumentation;
		}
	}
}