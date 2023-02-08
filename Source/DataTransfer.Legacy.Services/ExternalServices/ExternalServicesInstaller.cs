using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.ExternalServices.RetryPolicies;
using Relativity.Productions.Services.V2;

namespace Relativity.DataTransfer.Legacy.Services.ExternalServices
{
	internal static class ExternalServicesInstaller
	{
		public static void Install(IWindsorContainer container)
		{
			// external services facades registration
			container.Register(Component
				.For<IKeplerRetryPolicyFactory>()
				.ImplementedBy<RetryableKeplerErrorsRetryPolicyFactory>()
				.LifestyleSingleton());
			container.Register(Component
				.For<IProductionExternalService>()
				.ImplementedBy<ProductionExternalService>()
				.LifestyleTransient());
			container.Register(Component
				.For<Func<IProductionExternalService>>()
				.UsingFactoryMethod<Func<IProductionExternalService>>(kernel => kernel.Resolve<IProductionExternalService>)
				.LifestyleTransient());

			// external services registration
			container.Register(Component
				.For<IProductionManager>()
				.UsingFactoryMethod(kernel =>
					kernel
						.Resolve<IHelper>()
						.GetServicesManager()
						.CreateProxy<IProductionManager>(ExecutionIdentity.CurrentUser))
				.LifestyleTransient());
		}

	}
}
