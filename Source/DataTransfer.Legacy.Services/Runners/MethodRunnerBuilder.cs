namespace Relativity.DataTransfer.Legacy.Services.Runners
{
	public class MethodRunnerBuilder
	{
		public IMethodRunner Build()
		{
			var methodRunner = new MethodRunner();
			var errorHandling = new MethodRunnerWithErrorHandling(methodRunner);
			var toggle = new MethodRunnerWithToggleCheck(errorHandling);
			return toggle;
		}
	}
}