namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	public class HealthCheckResult
	{
		public HealthCheckResult(bool isHealthy, string message)
		{
			IsHealthy = isHealthy;
			Message = message;
		}

		public bool IsHealthy { get; }
		public string Message { get; }
	}
}
