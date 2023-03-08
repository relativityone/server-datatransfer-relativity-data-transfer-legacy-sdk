using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Windsor.Proxy;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Helpers.BatchCache;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using Relativity.DataTransfer.Legacy.Services.Observability;
using Relativity.DataTransfer.Legacy.Services.SQL;
using Relativity.Telemetry.APM;
using DataTransfer.Legacy.MassImport.Data;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry.MetricsEventsBuilders;
using Relativity.DataTransfer.Legacy.Services.ExternalServices;

namespace Relativity.DataTransfer.Legacy.Services.Installer
{
	public class ServicesInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Kernel.ProxyFactory = new DefaultProxyFactory(disableSignedModule: true);

			container.Register(Component.For<IAPILog>().UsingFactoryMethod(k => { return new RelEyeLogger(TelemetryConstants.Values.ServiceName, k.Resolve<IHelper>().GetLoggerFactory().GetLogger(), k.Resolve<IHelper>().GetInstanceSettingBundle()); }).LifestyleTransient());
			container.Register(Component.For<IAPM>().Instance(Client.APMClient));
			container.Register(Component.For<IServiceContextFactory>().ImplementedBy<ServiceContextFactory>());
			container.Register(Component.For<ICommunicationModeStorage>().ImplementedBy<CommunicationModeInstanceSettingStorage>());

			// interceptors registration
			container.Register(Component.For<LogInterceptor>().LifestyleTransient());
			container.Register(Component.For<MetricsInterceptor>().LifestyleTransient());
			container.Register(Component.For<DistributedTracingInterceptor>().LifestyleTransient());
			container.Register(Component.For<PermissionCheckInterceptor>().LifestyleTransient());
			container.Register(Component.For<UnhandledExceptionInterceptor>().LifestyleTransient());
			container.Register(Component.For<ToggleCheckInterceptor>().LifestyleTransient());

			container.Register(Component.For<IMetricsPublisher>().ImplementedBy<APMMetricsPublisher>().LifestyleTransient());
			container.Register(Component.For<IMetricsPublisher>().ImplementedBy<LoggingMetricsPublisher>().LifestyleTransient());
			container.Register(Component.For<IMetricsContext>().ImplementedBy<MetricsContext>().LifestyleTransient());
			container.Register(Component.For<ISnowflakeMetrics>().ImplementedBy<SnowflakeMetrics>().LifestyleTransient());
			container.Register(Component.For<Func<IMetricsContext>>().UsingFactoryMethod(x =>
				new Func<IMetricsContext>(container.Resolve<IMetricsContext>)));
			container.Register(Component.For<IRelativityPermissionHelper>().ImplementedBy<RelativityPermissionHelper>().LifestyleTransient());

			container.Register(Component.For<IInstanceSettingsBundle>()
				.UsingFactoryMethod((x, c) => x.Resolve<IHelper>().GetInstanceSettingBundle()));

			container.Register(Component.For<IBatchResultCache>().ImplementedBy<BatchResultCache>().LifestyleTransient());
			container.Register(Component.For<ISqlExecutor>().ImplementedBy<DbContextToSqlExecutorAdapter>().LifestyleTransient());
			container.Register(Component.For<ISqlRetryPolicy>().ImplementedBy<SqlRetryPolicy>().LifestyleTransient());
			container.Register(Component.For<ITraceGenerator>().ImplementedBy<TraceGenerator>().LifestyleSingleton());
			container.Register(Component.For<RetryPolicyFactory>());
			container.Register(Component.For<ITelemetryPublisher>().ImplementedBy<ApmTelemetryPublisher>().LifestyleTransient());
			container.Register(Component.For<IRelEyeMetricsService>().ImplementedBy<RelEyeMetricsService>().LifestyleTransient());
			container.Register(Component.For<IEventsBuilder>().ImplementedBy<EventsBuilder>().LifestyleTransient());
			container.Register(Component.For<IRedactedNativesValidator>().ImplementedBy<RedactedNativesValidator>().LifestyleTransient());
			container.Register(Component.For<IFileRepositoryExternalServiceFactory>().ImplementedBy<FileRepositoryExternalServiceFactory>().LifestyleTransient());
			container.Register(Component.For<IFileRepositoryExternalService>()
				.UsingFactoryMethod(x => x.Resolve<IFileRepositoryExternalServiceFactory>().Create()));


			StorageAccessProvider.InitializeStorageAccess(container);
			ExternalServicesInstaller.Install(container);
		}
	}
}