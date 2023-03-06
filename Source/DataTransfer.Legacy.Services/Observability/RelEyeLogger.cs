namespace Relativity.DataTransfer.Legacy.Services.Observability
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using global::DataTransfer.Legacy.MassImport.Core;
	using global::DataTransfer.Legacy.MassImport.RelEyeTelemetry;
	using Relativity.API;
	using Serilog;
	using Serilog.Core;
	using Serilog.Events;
	using Serilog.Sinks.OpenTelemetry;
	using ILogger = Serilog.ILogger;

	/// <summary>
	/// RelEyeLogger attributed for distributed tracing. Should be one per binary.
	/// </summary>
	public class RelEyeLogger : IAPILog
	{
		private readonly string serviceName;
		private readonly IAPILog relativityLogger;
		private readonly IInstanceSettingsBundle instanceSettingsBundle;
		private ILogger serilogLogger = Logger.None;
		private bool initialized = false;
		private object obj = new object();

		/// <summary>
		/// Initializes a new instance of the <see cref="RelEyeLogger"/> class.
		/// </summary>
		/// <param name="serviceName"></param>
		/// <param name="relativityLogger"></param>
		/// <param name="instanceSettingsBundle"></param>
		public RelEyeLogger(string serviceName, IAPILog relativityLogger, IInstanceSettingsBundle instanceSettingsBundle)
		{
			this.initialized = false;
			this.serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
			this.relativityLogger = relativityLogger ?? throw new ArgumentNullException(nameof(relativityLogger));
			this.instanceSettingsBundle = instanceSettingsBundle ?? throw new ArgumentNullException(nameof(instanceSettingsBundle));
		}

		private RelEyeLogger(string serviceName, IAPILog relativityLogger, ILogger serilogLogger)
		{
			this.initialized = true;
			this.serviceName = serviceName;
			this.relativityLogger = relativityLogger;
			this.serilogLogger = serilogLogger;
		}

		/// <inheritdoc/>
		public IAPILog ForContext<T>()
		{
			Initialize();
			return new RelEyeLogger(serviceName, relativityLogger.ForContext<T>(), serilogLogger.ForContext<T>());
		}

		/// <inheritdoc/>
		public IAPILog ForContext(Type source)
		{
			Initialize();
			return new RelEyeLogger(serviceName, relativityLogger.ForContext(source), serilogLogger.ForContext(source));
		}

		/// <inheritdoc/>
		public IAPILog ForContext(string propertyName, object value, bool destructureObjects)
		{
			Initialize();
			return new RelEyeLogger(serviceName, relativityLogger.ForContext(propertyName, value, destructureObjects), serilogLogger.ForContext(propertyName, value, destructureObjects));
		}

		/// <inheritdoc/>
		public IDisposable LogContextPushProperty(string propertyName, object obj)
		{
			Initialize();
			return relativityLogger.LogContextPushProperty(propertyName, obj);
		}

		/// <inheritdoc/>
		public void LogDebug(string messageTemplate, params object[] propertyValues)
		{
			Initialize();
			serilogLogger.Debug(messageTemplate, propertyValues);
			relativityLogger.LogDebug(messageTemplate, propertyValues);
		}

		/// <inheritdoc/>
		public void LogDebug(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Initialize();
			serilogLogger.Debug(exception, messageTemplate, propertyValues);
			relativityLogger.LogDebug(exception, messageTemplate, propertyValues);
		}

		/// <inheritdoc/>
		public void LogError(string messageTemplate, params object[] propertyValues)
		{
			Initialize();
			serilogLogger.Error(messageTemplate, propertyValues);
			relativityLogger.LogError(messageTemplate, propertyValues);
		}

		/// <inheritdoc/>
		public void LogError(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Initialize();
			serilogLogger.Error(exception, messageTemplate, propertyValues);
			relativityLogger.LogError(exception, messageTemplate, propertyValues);
		}

		/// <inheritdoc/>
		public void LogFatal(string messageTemplate, params object[] propertyValues)
		{
			Initialize();
			serilogLogger.Fatal(messageTemplate, propertyValues);
			relativityLogger.LogFatal(messageTemplate, propertyValues);
		}

		/// <inheritdoc/>
		public void LogFatal(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Initialize();
			serilogLogger.Fatal(exception, messageTemplate, propertyValues);
			relativityLogger.LogFatal(exception, messageTemplate, propertyValues);
		}

		/// <inheritdoc/>
		public void LogInformation(string messageTemplate, params object[] propertyValues)
		{
			Initialize();
			serilogLogger.Information(messageTemplate, propertyValues);
			relativityLogger.LogInformation(messageTemplate, propertyValues);
		}

		/// <inheritdoc/>
		public void LogInformation(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Initialize();
			serilogLogger.Information(exception, messageTemplate, propertyValues);
			relativityLogger.LogInformation(exception, messageTemplate, propertyValues);
		}

		/// <inheritdoc/>
		public void LogVerbose(string messageTemplate, params object[] propertyValues)
		{
			Initialize();
			serilogLogger.Verbose(messageTemplate, propertyValues);
			relativityLogger.LogVerbose(messageTemplate, propertyValues);
		}

		/// <inheritdoc/>
		public void LogVerbose(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Initialize();
			serilogLogger.Verbose(exception, messageTemplate, propertyValues);
			relativityLogger.LogVerbose(exception, messageTemplate, propertyValues);
		}

		/// <inheritdoc/>
		public void LogWarning(string messageTemplate, params object[] propertyValues)
		{
			Initialize();
			serilogLogger.Warning(messageTemplate, propertyValues);
			relativityLogger.LogWarning(messageTemplate, propertyValues);
		}

		/// <inheritdoc/>
		public void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues)
		{
			Initialize();
			serilogLogger.Warning(exception, messageTemplate, propertyValues);
			relativityLogger.LogWarning(exception, messageTemplate, propertyValues);
		}

		private void Initialize()
		{
			if (initialized)
			{
				return;
			}

			try
			{
				var apikey = instanceSettingsBundle.GetStringAsync(RelEyeSettings.RelativityTelemetrySection, RelEyeSettings.ReleyeTokenSettingName).GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(apikey))
				{
					relativityLogger.LogWarning($"Instance setting - Section:{RelEyeSettings.RelativityTelemetrySection}; Name:{RelEyeSettings.ReleyeTokenSettingName}; is missing. Cannot create RelEyeLogger.");
					return;
				}

				var otlpEndpoint = instanceSettingsBundle.GetStringAsync(RelEyeSettings.RelativityTelemetrySection, RelEyeSettings.ReleyeUriLogsName).GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(apikey))
				{
					relativityLogger.LogWarning($"Instance setting - Section:{RelEyeSettings.RelativityTelemetrySection}; Name:{RelEyeSettings.ReleyeUriLogsName}; is missing. Cannot create RelEyeLogger.");
					return;
				}

				var instanceIdentifier = instanceSettingsBundle.GetStringAsync(RelEyeSettings.RelativityCoreSection, RelEyeSettings.InstanceIdentifierSettingName).GetAwaiter().GetResult()?.ToLower();
				if (string.IsNullOrEmpty(apikey))
				{
					relativityLogger.LogWarning($"Instance setting - Section:{RelEyeSettings.RelativityTelemetrySection}; Name:{RelEyeSettings.InstanceIdentifierSettingName}; is missing. Cannot create RelEyeLogger.");
					return;
				}

				lock (obj)
				{
					if (initialized)
					{
						return;
					}

					serilogLogger = new LoggerConfiguration()
						.MinimumLevel.Information()
						.WriteTo.OpenTelemetry(
							endpoint: otlpEndpoint,
							protocol: OpenTelemetrySink.OtlpProtocol.HttpProtobuf,
							headers: new Dictionary<string, string>() { { "apikey", apikey }, },
							batchSizeLimit: 2,
							batchPeriod: 2,
							batchQueueLimit: 10)
						.Enrich.WithProperty(TelemetryConstants.AttributeNames.R1SourceID, instanceIdentifier.ToLower())
						.Enrich.WithProperty(TelemetryConstants.AttributeNames.ServiceNamespace, TelemetryConstants.Values.ServiceNamespace)
						.Enrich.WithProperty(TelemetryConstants.AttributeNames.ServiceName, serviceName)
						.Enrich.WithProperty(TelemetryConstants.AttributeNames.R1TeamID, TelemetryConstants.Values.R1TeamID)
						.Enrich.With(new TracingEnricher())
						.Enrich.FromLogContext()
						.CreateLogger();
					initialized = true;
				}
			}
			catch (Exception ex)
			{
				relativityLogger.LogError(ex, "Cannot Initialize RelEye Logger.");
			}
		}

		private class TracingEnricher : ILogEventEnricher
		{
			public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
			{
				logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(TelemetryConstants.AttributeNames.RelEyeTraceID, Activity.Current?.TraceId));
				logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(TelemetryConstants.AttributeNames.RelEyeSpanID, Activity.Current?.SpanId));
			}
		}
	}
}
