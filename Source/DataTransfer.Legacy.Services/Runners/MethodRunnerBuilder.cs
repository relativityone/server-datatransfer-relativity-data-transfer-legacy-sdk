namespace Relativity.DataTransfer.Legacy.Services.Runners
{
	public class MethodRunnerBuilder
	{
		public IMethodRunner Build()
		{
			var methodRunner = new MethodRunner();
			var toggle = new MethodRunnerWithToggleCheck(methodRunner);
			return toggle;
		}
	}
}