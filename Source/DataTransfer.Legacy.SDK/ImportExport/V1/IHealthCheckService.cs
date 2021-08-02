using System;
using System.Threading.Tasks;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Kepler.Services;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1
{
	[WebService("DataTransfer.Legacy Health Check Service")]
	[ServiceAudience(Audience.Private)]
	[RoutePrefix("health-check")]
	public interface IHealthCheckService : IDisposable
	{
		/// <summary>
		/// Asynchronously check health status of the services
		/// </summary>
		/// <returns>A HealthCheckResult with information about the application</returns>
		[HttpGet]
		[Route("")]
		Task<HealthCheckResult> HealthCheckAsync();
	}
}