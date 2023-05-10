using System;
using System.Runtime.CompilerServices;
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
			IHelper iHelper = null;

			var container = new WindsorContainer();
			try
			{
				container.Register(Component.For<IHelper>().UsingFactoryMethod(helperFactory, manageExternally)
					.LifestyleTransient());
				iHelper = container.Resolve<IHelper>();
				container.Register(Component.For<IRetryPolicyProvider>().ImplementedBy<RetryPolicyProvider>()
					.LifestyleTransient());
				container.Register(Component.For<IInstanceSettingsService>().ImplementedBy<InstanceSettingsService>()
					.LifestyleTransient());
				container.Register(Component.For<IAPILog>()
					.UsingFactoryMethod(k => iHelper.GetLoggerFactory().GetLogger()).LifestyleTransient());
				return container;
			}
			finally
			{
				container.Release(iHelper);
			}
			
		}
	}
}