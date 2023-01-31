// <copyright file="TraceGenerator.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

namespace Relativity.DataTransfer.Legacy.Services.Observability
{
	using global::DataTransfer.Legacy.MassImport.Core;
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
			
			ActivityListener listener = new ActivityListener()
			{
				ShouldListenTo = _ => true,
				Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
			};

			ActivitySource.AddActivityListener(listener);
		}

		public Activity StartActivity(string name, ActivityKind kind, ActivityContext parentContext, IEnumerable<KeyValuePair<string, object>> tags = null, IEnumerable<ActivityLink> links = null, DateTimeOffset startTime = default(DateTimeOffset))
		{
			if (tracerProvider == null)
            {
				try
				{
					ReleyeUriTraces =  _instanceSettingsBundle.GetStringAsync(TelemetryConstants.RelEyeSettings.RelativityTelemetrySection, TelemetryConstants.RelEyeSettings.ReleyeUriTracesSettingName).GetAwaiter().GetResult();
					ApiKey = _instanceSettingsBundle.GetStringAsync(TelemetryConstants.RelEyeSettings.RelativityTelemetrySection, TelemetryConstants.RelEyeSettings.ReleyeTokenSettingName).GetAwaiter().GetResult();
					
					tracerProvider = Sdk.CreateTracerProviderBuilder()
								.AddSource(new[] { TelemetryConstants.Application.SystemName })
								.SetResourceBuilder(
									ResourceBuilder.CreateDefault()
										.AddService(TelemetryConstants.Application.ServiceName))
								.SetSampler(new AlwaysOnSampler())
								.AddOtlpExporter(options =>
								{
									options.Endpoint = new Uri(ReleyeUriTraces);
									options.Headers = $"apikey={ApiKey}";
									options.Protocol = OtlpExportProtocol.HttpProtobuf;
								})
								.Build();

					activitySource = new ActivitySource(TelemetryConstants.Application.SystemName);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Cannot create tracerProvider");
					return null;
				}			
			}

			var activity = activitySource?.StartActivity(name, kind, parentContext, tags, links, startTime);
			SetSystemTags(activity);
			return activity;
		}

		private void SetSystemTags(Activity activity)
        {
			activity?.SetBaggage(TelemetryConstants.MetricsAttributes.OwnerTeamId, TelemetryConstants.Application.OwnerTeamId);
			activity?.SetBaggage(TelemetryConstants.MetricsAttributes.SystemName, TelemetryConstants.Application.SystemName);
			activity?.SetBaggage(TelemetryConstants.MetricsAttributes.ServiceName, TelemetryConstants.Application.ServiceName);
			activity?.SetBaggage(TelemetryConstants.MetricsAttributes.ApplicationID, TelemetryConstants.Application.ApplicationID);
			activity?.SetBaggage(TelemetryConstants.MetricsAttributes.ApplicationName, TelemetryConstants.Application.ApplicationName);

			activity?.SetTag(TelemetryConstants.MetricsAttributes.OwnerTeamId, TelemetryConstants.Application.OwnerTeamId);
			activity?.SetTag(TelemetryConstants.MetricsAttributes.SystemName, TelemetryConstants.Application.SystemName);
			activity?.SetTag(TelemetryConstants.MetricsAttributes.ServiceName, TelemetryConstants.Application.ServiceName);
			activity?.SetTag(TelemetryConstants.MetricsAttributes.ApplicationID, TelemetryConstants.Application.ApplicationID);
			activity?.SetTag(TelemetryConstants.MetricsAttributes.ApplicationName, TelemetryConstants.Application.ApplicationName);
		}

        public void Dispose()
        {
            activitySource?.Dispose();
            tracerProvider?.Dispose();
        }
    }
}