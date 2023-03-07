using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.ExternalServices.RetryPolicies;
using Relativity.Productions.Services.V2;
using Relativity.Services.Interfaces.ResourceServer;
using Relativity.Services.Interfaces.Workspace;

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

			// This is registered with ExecutionIdentity.System because we need te read ResourcePool and for client domains it was NULL for CurrentUser
			container.Register(Component
				.For<IWorkspaceManager>()
				.UsingFactoryMethod(kernel =>
					kernel
						.Resolve<IHelper>()
						.GetServicesManager()
						.CreateProxy<IWorkspaceManager>(ExecutionIdentity.System))
				.LifestyleTransient());

			// This is registered with ExecutionIdentity.System because we get NotAuthorizedException when reading fileshares as a CurrentUser
			container.Register(Component
				.For<IFileRepositoryServerManager>()
				.UsingFactoryMethod(kernel =>
					kernel
						.Resolve<IHelper>()
						.GetServicesManager()
						.CreateProxy<IFileRepositoryServerManager>(ExecutionIdentity.System))
				.LifestyleTransient());
		}

	}
}
