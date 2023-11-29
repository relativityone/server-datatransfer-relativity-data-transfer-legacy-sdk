using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Relativity.API;

namespace Relativity.DataTransfer.Legacy.PostInstallEventHandler
{
	public class PostInstallEventHandlerInstaller
	{
		/// <summary>
		/// Creating container for post install event handler.
		/// </summary>
		/// <param name="helperFactory">IHelper.</param>
		/// <returns>IWindsorContainer.</returns>
		public static IWindsorContainer CreateContainer(Func<IHelper> helperFactory)
		{
			const bool manageExternally = true;

			var container = new WindsorContainer();
			container.Register(Component.For<IHelper>().UsingFactoryMethod(helperFactory, manageExternally).LifestyleTransient());

			container.Register(Component.For<IRetryPolicyProvider>().ImplementedBy<RetryPolicyProvider>().LifestyleTransient());
			container.Register(Component.For<IInstanceSettingsService>().ImplementedBy<InstanceSettingsService>().LifestyleTransient());
			container.Register(Component.For<IAPILog>().UsingFactoryMethod(k => k.Resolve<IHelper>().GetLoggerFactory().GetLogger()).LifestyleTransient());
			return container;
		}
	}
}