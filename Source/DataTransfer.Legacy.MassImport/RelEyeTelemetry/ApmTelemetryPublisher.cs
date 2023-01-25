// <copyright file="ApmTelemetryPublisher.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry
{
	using System.Collections.Generic;
	using Relativity.Telemetry.APM;

	/// <inheritdoc/>
	public class ApmTelemetryPublisher : ITelemetryPublisher
	{
		private readonly IAPM _apm;

		/// <summary>
		/// Initializes a new instance of the <see cref="ApmTelemetryPublisher"/> class.
		/// </summary>
		/// <param name="apm"></param>
		public ApmTelemetryPublisher(IAPM apm)
		{
			this._apm = apm;
		}

		/// <inheritdoc/>
		public void PublishEvent(string name, Dictionary<string, object> attributes)
		{
			var result = new HealthCheckOperationResult();
			IHealthMeasure apmHealthCheck = this._apm.HealthCheckOperation(name, () => result, customData: attributes);
			apmHealthCheck.Write();
		}
	}
}