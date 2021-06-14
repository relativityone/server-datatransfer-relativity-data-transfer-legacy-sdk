﻿using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using Relativity.DataTransfer.Legacy.Services.Runners;
using Relativity.Telemetry.APM;

namespace Relativity.DataTransfer.Legacy.Services.Installer
{
	public class ServicesInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(Component.For<IAPILog>()
				.UsingFactoryMethod((x, c) => x.Resolve<IHelper>().GetLoggerFactory().GetLogger()));
			container.Register(Component.For<IAPM>().Instance(Client.APMClient));
			container.Register(Component.For<MethodRunnerBuilder>().ImplementedBy<MethodRunnerBuilder>());
			container.Register(Component.For<IMethodRunner>()
				.UsingFactoryMethod((x, c) => x.Resolve<MethodRunnerBuilder>().Build()));
			container.Register(Component.For<IServiceContextFactory>().ImplementedBy<ServiceContextFactory>());
			
			// interceptors registration
			container.Register(Component.For<LogInterceptor>().LifestyleTransient());
			container.Register(Component.For<MetricsInterceptor>().LifestyleTransient());
			container.Register(Component.For<PermissionCheckInterceptor>().LifestyleTransient());
			
			container.Register(Component.For<IMetricsPublisher>().ImplementedBy<APMMetricsPublisher>().LifestyleTransient());
			container.Register(Component.For<IMetricsPublisher>().ImplementedBy<LoggingMetricsPublisher>().LifestyleTransient());
			container.Register(Component.For<IMetricsContext>().ImplementedBy<MetricsContext>().LifestyleTransient());
			container.Register(Component.For<Func<IMetricsContext>>().UsingFactoryMethod(x =>
				new Func<IMetricsContext>(container.Resolve<IMetricsContext>)));
			container.Register(Component.For<IRelativityPermissionHelper>().ImplementedBy<RelativityPermissionHelper>().LifestyleTransient());

			container.Register(Component.For<IInstanceSettingsBundle>()
				.UsingFactoryMethod((x, c) => x.Resolve<IHelper>().GetInstanceSettingBundle()));
		}
	}
}