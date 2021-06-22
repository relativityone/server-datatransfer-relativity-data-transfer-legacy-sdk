using System.Linq;
using Castle.Core;
using FluentAssertions;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.Services;
using Relativity.DataTransfer.Legacy.Services.Interceptors;

namespace Relativity.DataTransfer.Legacy.Tests
{
	[TestFixture]
	public class PermissionInterceptorDecorationTests
	{
		[Test]
		public void ShouldHaveAllServiceClassesAttributedWithPermissionInterceptor()
		{
			var services = typeof(BaseService).Assembly.GetTypes().Where(x => x.Name.EndsWith("Service") && !x.Name.EndsWith(nameof(BaseService)));
			foreach (var service in services)
			{
				service.GetCustomAttributes(typeof(InterceptorAttribute), true).Any(x =>
					((InterceptorAttribute) x).Interceptor.ToString().EndsWith(nameof(PermissionCheckInterceptor)))
					.Should().BeTrue();
			}
		}
	}
}
