// <copyright file="IMetricsContext.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

namespace Relativity.DataTransfer.Legacy.Services.Metrics
{
	using OpenTelemetry;
	using OpenTelemetry.Exporter;
	using OpenTelemetry.Resources;
	using OpenTelemetry.Trace;
	using System;
	using System.Diagnostics;

	public class CoreTraceGenerator : ITraceGenerator
	{
		private const string ServiceName = "data-transfer-legacy-rap-kepler-api";
		private TracerProvider tracerProvider = null;
		private ActivitySource activitySource = new ActivitySource(ServiceName);

		public CoreTraceGenerator()
		{
			tracerProvider = Sdk.CreateTracerProviderBuilder()
			.AddSource(new[] { ServiceName })
			.SetResourceBuilder(
				ResourceBuilder.CreateDefault()
					.AddService(ServiceName))
			.SetSampler(new AlwaysOnSampler())
			.AddOtlpExporter(options =>
			{
				options.Endpoint = new Uri("https://services.ctus.reg.k8s.r1.kcura.com/releye/v1/traces");
				options.Headers = $"apikey=VzUDvGtIZJ_KlQmq0ihOnsp1o52Tcr";
				options.Protocol = OtlpExportProtocol.HttpProtobuf;
			})
			.AddHttpClientInstrumentation()
			.Build();
		}

		public ActivitySource GetActivitySurce()
		{
			return activitySource;
		}

		public void SetSystemTags(Activity activity)
		{
			activity?.SetTag("r1.team.id", "PTCI-4941411");
			activity?.SetTag("service.namespace", "data-transfer-legacy-rap");
			activity?.SetTag("service.name", ServiceName);
			activity?.SetTag("application.guid", "9f9d45ff-5dcd-462d-996d-b9033ea8cfce");
			activity?.SetTag("application.name", "DataTranfer.Legacy");

			activity?.SetBaggage("r1.team.id", "PTCI-4941411");
			activity?.SetBaggage("service.namespace", "data-transfer-legacy-rap");
			activity?.SetBaggage("service.name", ServiceName);
			activity?.SetBaggage("application.guid", "9f9d45ff-5dcd-462d-996d-b9033ea8cfce");
			activity?.SetBaggage("application.name", "DataTranfer.Legacy");
		}

		public void Dispose()
		{
			activitySource?.Dispose();
			tracerProvider?.Dispose();
		}
	}
}