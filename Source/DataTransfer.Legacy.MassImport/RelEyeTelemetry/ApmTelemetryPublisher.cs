using Relativity.Telemetry.APM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry
{
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
