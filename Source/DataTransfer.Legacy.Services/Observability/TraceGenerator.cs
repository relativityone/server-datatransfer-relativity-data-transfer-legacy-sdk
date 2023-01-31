// <copyright file="TraceGenerator.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

namespace Relativity.DataTransfer.Legacy.Services.Observability
{
    using OpenTelemetry;
    using OpenTelemetry.Exporter;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;
	using Relativity.API;
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

		private const string RelativityTelemetrySection = "Relativity.Telemetry";
		private const string ReleyeUriTracesSettingName = "ReleyeUriTraces";
		private const string ReleyeTokenSettingName = "ReleyeToken";

		private string ApiKey = string.Empty;
		private string ReleyeUriTraces = string.Empty;

		private readonly IAPILog _logger;
		private readonly IInstanceSettingsBundle _instanceSettingsBundle;

		private TracerProvider tracerProvider = null;
		private ActivitySource activitySource = null;

		public TraceGenerator(IAPILog logger, IInstanceSettingsBundle instanceSettingsBundle)
		{
			_logger = logger ?? throw new NullReferenceException(nameof(logger));
			_instanceSettingsBundle = instanceSettingsBundle ?? throw new NullReferenceException(nameof(instanceSettingsBundle));
		}

		public Activity StartActivity(string name, ActivityKind kind, ActivityContext parentContext, IEnumerable<KeyValuePair<string, object>> tags = null, IEnumerable<ActivityLink> links = null, DateTimeOffset startTime = default(DateTimeOffset))
		{
			if (tracerProvider == null)
            {
				try
				{
					ReleyeUriTraces =  _instanceSettingsBundle.GetStringAsync(RelativityTelemetrySection, ReleyeUriTracesSettingName).GetAwaiter().GetResult();
					ApiKey = _instanceSettingsBundle.GetStringAsync(RelativityTelemetrySection, ReleyeTokenSettingName).GetAwaiter().GetResult();
					
					tracerProvider = Sdk.CreateTracerProviderBuilder()
								.AddSource(new[] { SystemName })
								.SetResourceBuilder(
									ResourceBuilder.CreateDefault()
										.AddService(ServiceName))
								.SetSampler(new AlwaysOnSampler())
								.AddOtlpExporter(options =>
								{
									options.Endpoint = new Uri(ReleyeUriTraces);
									options.Headers = $"apikey={ApiKey}";
									options.Protocol = OtlpExportProtocol.HttpProtobuf;
								})
								.Build();

					activitySource = new ActivitySource(SystemName);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Cannot create tracerProvider");
				}			
			}

			var activity = activitySource?.StartActivity(name, kind, parentContext, tags, links, startTime);
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