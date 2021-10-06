using System.Linq;
using Castle.Core;
using FluentAssertions;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.Services;
using Relativity.DataTransfer.Legacy.Services.Interceptors;

namespace Relativity.DataTransfer.Legacy.Tests
{
	[TestFixture]
	public class UnhandledExceptionInterceptorDecorationTests
	{
		[Test]
		public void ShouldHaveAllServiceClassesAttributedWithPermissionInterceptor()
		{
			var services = typeof(BaseService).Assembly
				.GetTypes()
				.Where(t => t.IsSubclassOf(typeof(BaseService)) && !t.IsAbstract);
			foreach (var service in services)
			{
				var attributes =  service.GetCustomAttributes(inherit: true);
				attributes.Length.Should().BeGreaterThan(0, "Service should have interceptors");

				var unhandledExceptionAttribute = attributes.FirstOrDefault(x =>
					((InterceptorAttribute) x).Interceptor.ToString().EndsWith(nameof(UnhandledExceptionInterceptor)));

				unhandledExceptionAttribute.Should().NotBeNull($"Service {service} should have UnhandledExceptionInterceptor");

				var firstAttribute = attributes.First();
				((InterceptorAttribute)firstAttribute).Interceptor.ToString()
					.Should()
					.EndWith(nameof(UnhandledExceptionInterceptor), "UnhandledExceptionInterceptor should be the first interceptor on the list");
			}
		}
	}
}
