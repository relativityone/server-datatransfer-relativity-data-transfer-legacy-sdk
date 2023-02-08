using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.ExternalServices;

namespace Relativity.DataTransfer.Legacy.Services.Tests.ExternalServices
{
	[TestFixture]
	public class ExternalServicesInstallerTests
	{
		private IWindsorContainer _container;

		[SetUp]
		public void SetUp()
		{
			_container = new WindsorContainer();
			RegisterMock<IHelper>();
			RegisterMock<IAPILog>();

			ExternalServicesInstaller.Install(_container);
		}

		[Test]
		public void Container_ShouldResolveProductionExternalService_WhenInstallWasInvoked()
		{
			// act
			var actual = _container.Resolve<IProductionExternalService>();

			// assert
			actual.Should().BeAssignableTo<ProductionExternalService>();
		}

		private void RegisterMock<T>() where T : class
		{
			_container
				.Register(Component.For<T>()
					.Instance(new Mock<T> { DefaultValue = DefaultValue.Mock }.Object));
		}
	}
}
