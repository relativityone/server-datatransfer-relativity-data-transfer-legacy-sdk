using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.Services.Installer;
using Relativity.Toggles;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Installer
{
	[TestFixture]
	public class ServicesInstallerTests
	{
		private IWindsorContainer _container;

		[SetUp]
		public void SetUp()
		{
			_container = new WindsorContainer();
			_container
				.Register(Component.For<IHelper>()
					.Instance(new Mock<IServiceHelper> { DefaultValue = DefaultValue.Mock }.Object));
			_container
				.Register(Component.For<IToggleProvider>()
					.Instance(new Mock<IToggleProvider> { DefaultValue = DefaultValue.Mock }.Object));

			var installer = new ServicesInstaller();
			installer.Install(_container, new DefaultConfigurationStore());
		}

		[Test]
		public void Container_ShouldResolveSearchService_WhenItWasRegisteredAndInstallWasInvoked()
		{
			Container_ShouldResolveService_WhenItWasRegisteredAndInstallWasInvoked<ISearchService, SearchService>();
		}

		[Test]
		public void Container_ShouldResolveProductionService_WhenItWasRegisteredAndInstallWasInvoked()
		{
			Container_ShouldResolveService_WhenItWasRegisteredAndInstallWasInvoked<IProductionService, ProductionService>();
		}

		private void Container_ShouldResolveService_WhenItWasRegisteredAndInstallWasInvoked<TInterface, TImpl>()
			where TInterface : class
			where TImpl : TInterface
		{
			// arrange
			_container.Register(Component.For<TInterface>().ImplementedBy<TImpl>());

			// act
			var actual = _container.Resolve<TInterface>();

			// assert
			actual.Should().NotBeNull();
		}
	}
}
