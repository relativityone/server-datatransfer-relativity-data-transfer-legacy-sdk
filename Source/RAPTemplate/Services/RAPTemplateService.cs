using System.Threading.Tasks;
using Relativity.Telemetry.APM;

namespace RAPTemplate.Services
{
	public class RAPTemplateService : IRAPTemplateService
	{
		public Task<bool> IsAlive()
		{
			return Task.FromResult(true);
		}

		public async Task<HealthCheckOperationResult> FailHealthCheckAsync()
		{
			return await Task.FromResult(RunHealthCheck(new SickRAPTemplateHealthCheck()));
		}

		public async Task<HealthCheckOperationResult> PassHealthCheckAsync()
		{
			return await Task.FromResult(RunHealthCheck(new HealthyRAPTemplateHealthCheck()));
		}

		private HealthCheckOperationResult RunHealthCheck(IHealthCheck healthCheck)
		{
			HealthCheckOperationResult result = healthCheck.CheckHealth();
			IHealthMeasure healthMeasure = Client.APMClient.HealthCheckOperation("RAPTemplateStatus", () => result);
			healthMeasure.Write();

			return result;
		}

		#region IDisposable Support
		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
