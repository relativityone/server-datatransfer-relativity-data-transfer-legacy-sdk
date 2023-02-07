using System.Linq;
using Castle.Core;
using FluentAssertions;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.Services;
using Relativity.DataTransfer.Legacy.Services.ExternalServices;
using Relativity.DataTransfer.Legacy.Services.Interceptors;

namespace Relativity.DataTransfer.Legacy.Tests
{
	[TestFixture]
	public class PermissionInterceptorDecorationTests
	{
		[Test]
		public void ShouldHaveAllServiceClassesAttributedWithPermissionInterceptor()
		{
			var servicesToExclude = new[] {
				typeof(BaseService),
				typeof(HealthCheckService),
				typeof(TAPIService),
				typeof(IProductionExternalService),
				typeof(ProductionExternalService),
			};

			var services = typeof(BaseService).Assembly
							.GetTypes()
							.Where(x => x.Name.EndsWith("Service"))
							.Except(servicesToExclude);
			foreach (var service in services)
			{
				service.GetCustomAttributes(typeof(InterceptorAttribute), true)
					.Any(x =>
					((InterceptorAttribute)x).Interceptor.ToString()
					.EndsWith(nameof(PermissionCheckInterceptor)))
					.Should().BeTrue($"because '{service}' should have attribute");
			}
		}
	}
}
