using Relativity.Telemetry.APM;

namespace RAPTemplate.Services
{
	public class SickRAPTemplateHealthCheck : IHealthCheck
	{
		public HealthCheckOperationResult CheckHealth()
		{
			return new HealthCheckOperationResult(false, "RAPTemplate Service is not healthy");
		}
	}
}
