using System;
using System.Threading.Tasks;
using Relativity.Kepler.Services;
using Relativity.Telemetry.APM;

namespace RAPTemplate.Services
{
	[WebService("RAPTemplate Service Manager")]
	[ServiceAudience(Audience.Public)]
	public interface IRAPTemplateService : IDisposable
	{
		Task<bool> IsAlive();
		Task<HealthCheckOperationResult> FailHealthCheckAsync();
		Task<HealthCheckOperationResult> PassHealthCheckAsync();
	}
}
