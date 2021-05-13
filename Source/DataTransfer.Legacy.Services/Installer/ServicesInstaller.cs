using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.Helpers;
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
		}
	}
}