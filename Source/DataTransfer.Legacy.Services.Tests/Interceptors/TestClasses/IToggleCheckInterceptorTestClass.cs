using Castle.Core;
using Relativity.DataTransfer.Legacy.Services.Interceptors;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Interceptors.TestClasses
{
	public interface IToggleCheckInterceptorTestClass
	{
		void Execute();
	}

	[Interceptor(typeof(ToggleCheckInterceptor))]
	public class ToggleCheckInterceptorTestClass : IToggleCheckInterceptorTestClass
	{
		public void Execute()
		{
		}
	}
}