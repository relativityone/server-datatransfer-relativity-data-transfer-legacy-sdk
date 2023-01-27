using System;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Core;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using Relativity.DataTransfer.Legacy.Services.Observability;
using Relativity.Services.Objects.Exceptions;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests
{
    [TestFixture]
	public class IAPICommunicationModeServiceTests
	{
		private Mock<ICommunicationModeStorage> _communicationModeStorageMock;
		private IIAPICommunicationModeService _uut;
		private Mock<IServiceContextFactory> _serviceContextFactoryMock;
		private Mock<IRelativityPermissionHelper> _relativityPermissionHelperMock;
		private Mock<IAPILog> _loggerMock;
		private Mock<IMetricsContext> _metricsMock;

		[SetUp]
		public void SetUp()
		{
			_serviceContextFactoryMock = new Mock<IServiceContextFactory>();
			_communicationModeStorageMock = new Mock<ICommunicationModeStorage>();
			_relativityPermissionHelperMock = new Mock<IRelativityPermissionHelper>();
			_loggerMock = new Mock<IAPILog>();
			_metricsMock = new Mock<IMetricsContext>();

			var container = new WindsorContainer();
			container.Register(Component.For<IAPILog>().Instance(_loggerMock.Object));
			container.Register(Component.For<IServiceContextFactory>().Instance(_serviceContextFactoryMock.Object));
			container.Register(Component.For<ICommunicationModeStorage>().Instance(_communicationModeStorageMock.Object));
			container.Register(Component.For<IRelativityPermissionHelper>().Instance(_relativityPermissionHelperMock.Object));
			container.Register(Component.For<ToggleCheckInterceptor>());
			container.Register(Component.For<PermissionCheckInterceptor>());
			container.Register(Component.For<LogInterceptor>());
			container.Register(Component.For<MetricsInterceptor>());
			container.Register(Component.For<UnhandledExceptionInterceptor>());
			container.Register(Component.For<DistributedTracingInterceptor>());
			container.Register(Component.For<IMetricsContext>().Instance(_metricsMock.Object));
			container.Register(Component.For<Func<IMetricsContext>>().UsingFactoryMethod(x =>
				new Func<IMetricsContext>(container.Resolve<IMetricsContext>)));
			container.Register(Component.For<IIAPICommunicationModeService>().ImplementedBy<IAPICommunicationModeService>());
			container.Register(Component.For<ITraceGenerator>().ImplementedBy<TraceGenerator>());

			_uut = container.Resolve<IIAPICommunicationModeService>();
		}

		[Test]
		public async Task ShouldReturnWebAPIModeAndLogWhenReadingModeThrowsException()
		{
			_communicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, Any.OtherThan(IAPICommunicationMode.ForceWebAPI))));
			var baseServiceContext = Any.InstanceOf<BaseServiceContext>();
			_serviceContextFactoryMock.Setup(x => x.GetBaseServiceContext(It.IsAny<int>()))
				.Returns(baseServiceContext);
			_relativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientImport)).Returns(true);
			_relativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientExport)).Returns(true);
			_communicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Throws(Any.Exception());
			_loggerMock.Setup(x => x.LogWarning(It.IsAny<string>()));
			var storageKey = Any.String();
			_communicationModeStorageMock.Setup(x => x.GetStorageKey())
				.Returns(storageKey);

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(IAPICommunicationMode.WebAPI);
			_loggerMock.Verify(x => x.LogWarning($"'{storageKey}' toggle not found. WebAPI IAPI communication mode will be used."), Times.Once);
		}

		[Test]
		public async Task ShouldReturnWebAPIModeAndLogWhenReadingModeReturnsNoMode()
		{
			_communicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, Any.OtherThan(IAPICommunicationMode.ForceWebAPI))));
			var baseServiceContext = Any.InstanceOf<BaseServiceContext>();
			_serviceContextFactoryMock.Setup(x => x.GetBaseServiceContext(It.IsAny<int>()))
				.Returns(baseServiceContext);
			_relativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientImport)).Returns(true);
			_relativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientExport)).Returns(true);
			_communicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((false, Any.ValueOf<IAPICommunicationMode>())));
			_loggerMock.Setup(x => x.LogWarning(It.IsAny<string>()));
			var storageKey = Any.String();
			_communicationModeStorageMock.Setup(x => x.GetStorageKey())
				.Returns(storageKey);

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(IAPICommunicationMode.WebAPI);
			_loggerMock.Verify(x => x.LogWarning($"Invalid IAPI communication mode in '{storageKey}' toggle. WebAPI IAPI communication mode will be used."), Times.Once);
		}

		[Test]
		public async Task ShouldReturnModeModeAndLogWhenReadingModeReturnsModeAndToggleReturnsModeOtherThanForceWebApi()
		{
			_communicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, Any.OtherThan(IAPICommunicationMode.ForceWebAPI))));
			var baseServiceContext = Any.InstanceOf<BaseServiceContext>();
			_serviceContextFactoryMock.Setup(x => x.GetBaseServiceContext(It.IsAny<int>()))
				.Returns(baseServiceContext);
			_relativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientImport)).Returns(true);
			_relativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientExport)).Returns(true);
			var mode = Any.ValueOf<IAPICommunicationMode>();
			_communicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, mode)));
			_loggerMock.Setup(x => x.LogWarning(It.IsAny<string>()));

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(mode);
		}

		[Test]
		public async Task ShouldReturnModeModeAndLogWhenReadingModeReturnsModeAndToggleReturnsForceWebApi()
		{
			_communicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, IAPICommunicationMode.ForceWebAPI)));
			var baseServiceContext = Any.InstanceOf<BaseServiceContext>();
			_serviceContextFactoryMock.Setup(x => x.GetBaseServiceContext(It.IsAny<int>()))
				.Returns(baseServiceContext);
			_relativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientImport)).Returns(true);
			_relativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientExport)).Returns(true);
			var mode = Any.ValueOf<IAPICommunicationMode>();
			_communicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, mode)));
			_loggerMock.Setup(x => x.LogWarning(It.IsAny<string>()));

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(mode);
		}
	}
}
