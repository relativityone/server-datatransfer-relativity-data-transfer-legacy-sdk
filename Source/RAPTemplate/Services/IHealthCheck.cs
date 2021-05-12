using Relativity.Telemetry.APM;

namespace RAPTemplate.Services
{
	public interface IHealthCheck
	{
		HealthCheckOperationResult CheckHealth();
	}
}
