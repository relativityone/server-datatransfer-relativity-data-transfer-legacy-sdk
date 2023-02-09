// <copyright file="TraceGenerator.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

namespace Relativity.DataTransfer.Legacy.Services.Observability
{
	using global::DataTransfer.Legacy.MassImport.Core;
	using global::DataTransfer.Legacy.MassImport.RelEyeTelemetry;
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
		private string SourceID = string.Empty;

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
					ReleyeUriTraces =  _instanceSettingsBundle.GetStringAsync(RelEyeSettings.RelativityTelemetrySection, RelEyeSettings.ReleyeUriTracesSettingName).GetAwaiter().GetResult();
					ApiKey = _instanceSettingsBundle.GetStringAsync(RelEyeSettings.RelativityTelemetrySection, RelEyeSettings.ReleyeTokenSettingName).GetAwaiter().GetResult();
					SourceID = _instanceSettingsBundle.GetStringAsync(RelEyeSettings.RelativityCoreSection, RelEyeSettings.InstanceIdentifierSettingName).GetAwaiter().GetResult()?.ToLower();

					tracerProvider = Sdk.CreateTracerProviderBuilder()
								.AddSource(new[] { TelemetryConstants.Values.ServiceNamespace })
								.SetResourceBuilder(
									ResourceBuilder.CreateDefault()
										.AddService(TelemetryConstants.Values.ServiceName))
								.SetSampler(new AlwaysOnSampler())
								.AddOtlpExporter(options =>
								{
									options.Endpoint = new Uri(ReleyeUriTraces);
									options.Headers = $"apikey={ApiKey}";
									options.Protocol = OtlpExportProtocol.HttpProtobuf;
								})
								.Build();

					activitySource = new ActivitySource(TelemetryConstants.Values.ServiceNamespace);
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
			activity?.SetBaggage(TelemetryConstants.AttributeNames.R1TeamID, TelemetryConstants.Values.R1TeamID);
			activity?.SetBaggage(TelemetryConstants.AttributeNames.ServiceNamespace, TelemetryConstants.Values.ServiceNamespace);
			activity?.SetBaggage(TelemetryConstants.AttributeNames.ServiceName, TelemetryConstants.Values.ServiceName);
			activity?.SetBaggage(TelemetryConstants.AttributeNames.ApplicationID, TelemetryConstants.Values.ApplicationID);
			activity?.SetBaggage(TelemetryConstants.AttributeNames.ApplicationName, TelemetryConstants.Values.ApplicationName);
			activity?.SetBaggage(TelemetryConstants.AttributeNames.R1SourceID, SourceID);
			
			activity?.SetTag(TelemetryConstants.AttributeNames.R1TeamID, TelemetryConstants.Values.R1TeamID);
			activity?.SetTag(TelemetryConstants.AttributeNames.ServiceNamespace, TelemetryConstants.Values.ServiceNamespace);
			activity?.SetTag(TelemetryConstants.AttributeNames.ServiceName, TelemetryConstants.Values.ServiceName);
			activity?.SetTag(TelemetryConstants.AttributeNames.ApplicationID, TelemetryConstants.Values.ApplicationID);
			activity?.SetTag(TelemetryConstants.AttributeNames.ApplicationName, TelemetryConstants.Values.ApplicationName);
			activity?.SetTag(TelemetryConstants.AttributeNames.R1SourceID, SourceID);
		}

        public void Dispose()
        {
            activitySource?.Dispose();
            tracerProvider?.Dispose();
        }
    }
}