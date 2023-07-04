using System.Collections.Generic;
using OpenTelemetry.Trace;

namespace Relativity.DataTransfer.Legacy.Services.Observability
{
	internal class OpenTelemetrySampler : Sampler
	{
		private readonly Sampler _baseSampler;
		private readonly HashSet<string> excludedPaths = new HashSet<string>
		{
			"/api/live",
			"/api/GetServiceStatus",
			"/api/ready"
		};

		public OpenTelemetrySampler(Sampler baseSampler)
		{
			_baseSampler = baseSampler;
		}

		public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
		{
			if (excludedPaths.Contains(samplingParameters.Name))
			{
				return new SamplingResult(SamplingDecision.Drop);
			}

			return _baseSampler.ShouldSample(samplingParameters);
		}
	}
}
