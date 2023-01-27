// <copyright file="TraceGenerator.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

namespace Relativity.DataTransfer.Legacy.Services.Observability
{
    using OpenTelemetry;
    using OpenTelemetry.Exporter;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class TraceGenerator : ITraceGenerator
    { 
		private const string TeamID = "PTCI-4941411";
		private const string SystemName = "data-transfer-legacy-rap";
		private const string ServiceName = "data-transfer-legacy-rap-kepler-api";
		private const string ApplicationID = "9f9d45ff-5dcd-462d-996d-b9033ea8cfce";
		private const string ApplicationName = "DataTranfer.Legacy";

		private const string ApiKey = "apikey=VzUDvGtIZJ_KlQmq0ihOnsp1o52Tcr";
		private const string RelEyeUrl = "https://services.ctus.reg.k8s.r1.kcura.com/releye/v1/traces";

		private TracerProvider tracerProvider = null;
		private ActivitySource activitySource = null;

		public Activity StartActivity(string name, ActivityKind kind, ActivityContext parentContext, IEnumerable<KeyValuePair<string, object>> tags = null, IEnumerable<ActivityLink> links = null, DateTimeOffset startTime = default(DateTimeOffset))
		{
			if (tracerProvider == null)
            {
				tracerProvider = Sdk.CreateTracerProviderBuilder()
							.AddSource(new[] { SystemName })
							.SetResourceBuilder(
								ResourceBuilder.CreateDefault()
									.AddService(ServiceName))
							.SetSampler(new AlwaysOnSampler())
							.AddOtlpExporter(options =>
							{
								options.Endpoint = new Uri(RelEyeUrl);
								options.Headers = ApiKey;
								options.Protocol = OtlpExportProtocol.HttpProtobuf;
                            })
							.Build();

				activitySource = new ActivitySource(SystemName);
			}

			var activity = activitySource.StartActivity(name, kind, parentContext, tags, links, startTime);
			SetSystemTags(activity);
			return activity;
		}

		public void SetSystemTags(Activity activity)
        {
			activity?.SetTag("r1.team.id", TeamID);
			activity?.SetTag("service.namespace", SystemName);
			activity?.SetTag("service.name", ServiceName);
			activity?.SetTag("application.guid", ApplicationID);
			activity?.SetTag("application.name", ApplicationName);

			activity?.SetBaggage("r1.team.id", TeamID);
			activity?.SetBaggage("service.namespace", SystemName);
			activity?.SetBaggage("service.name", ServiceName);
			activity?.SetBaggage("application.guid", ApplicationID);
			activity?.SetBaggage("application.name", ApplicationName);
		}

        public void Dispose()
        {
            activitySource?.Dispose();
            tracerProvider?.Dispose();
        }
    }
}