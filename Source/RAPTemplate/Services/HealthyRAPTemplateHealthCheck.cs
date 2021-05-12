using Relativity.Telemetry.APM;

namespace RAPTemplate.Services
{
	public class HealthyRAPTemplateHealthCheck : IHealthCheck
	{
		public HealthCheckOperationResult CheckHealth()
		{
			return new HealthCheckOperationResult(true, "RAPTemplate Service is healthy!");
		}
	}
}
