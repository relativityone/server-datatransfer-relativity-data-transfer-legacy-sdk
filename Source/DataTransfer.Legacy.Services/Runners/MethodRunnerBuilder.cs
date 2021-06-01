using Relativity.DataTransfer.Legacy.Services.Helpers;

namespace Relativity.DataTransfer.Legacy.Services.Runners
{
	public class MethodRunnerBuilder
	{
		private readonly IServiceContextFactory _serviceContextFactory;

		public MethodRunnerBuilder(IServiceContextFactory serviceContextFactory)
		{
			_serviceContextFactory = serviceContextFactory;
		}

		public IMethodRunner Build()
		{
			var methodRunner = new MethodRunner();
			var permissions = new MethodRunnerWithPermissionCheck(methodRunner, _serviceContextFactory);
			var errorHandling = new MethodRunnerWithErrorHandling(permissions);
			var toggle = new MethodRunnerWithToggleCheck(errorHandling);
			return toggle;
		}
	}
}