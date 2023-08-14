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
using Relativity.DataTransfer.Legacy.Services.Helpers.BatchCache;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using Relativity.DataTransfer.Legacy.Services.SQL;
using Relativity.Services.Exceptions;
using Relativity.Services.Objects.Exceptions;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests
{
	[TestFixture]
	public class BulkImportServiceTests
	{
		private Mock<IServiceContextFactory> _serviceContextFactoryMock;
		private Mock<ICommunicationModeStorage> _communicationModeStorageMock;
		private Mock<IRelativityPermissionHelper> _relativityPermissionHelperMock;
		private Mock<IAPILog> _loggerMock;
		private Mock<IMetricsContext> _metricsMock;
		private Mock<ISnowflakeMetrics> _snowflakeMetricsMock;
		private Mock<IHelper> _helperMock;
		private Mock<ISqlExecutor> _sqlExecutorMock;
		private Mock<ISqlRetryPolicy> _sqlRetryPolicyMock;
		private IBulkImportService _uut;

		[SetUp]
		public void SetUp()
		{
			_serviceContextFactoryMock = new Mock<IServiceContextFactory>();
			_communicationModeStorageMock = new Mock<ICommunicationModeStorage>();
			_relativityPermissionHelperMock = new Mock<IRelativityPermissionHelper>();
			_snowflakeMetricsMock = new Mock<ISnowflakeMetrics>();
			_loggerMock = new Mock<IAPILog>();
			_metricsMock = new Mock<IMetricsContext>();
			_helperMock = new Mock<IHelper>();
			_sqlExecutorMock = new Mock<ISqlExecutor>();
			_sqlRetryPolicyMock = new Mock<ISqlRetryPolicy>();

			var container = new WindsorContainer();
			container.Register(Component.For<IAPILog>().Instance(_loggerMock.Object));
			container.Register(Component.For<IServiceContextFactory>().Instance(_serviceContextFactoryMock.Object));
			container.Register(Component.For<ICommunicationModeStorage>().Instance(_communicationModeStorageMock.Object));
			container.Register(Component.For<IRelativityPermissionHelper>().Instance(_relativityPermissionHelperMock.Object));
			container.Register(Component.For<ISnowflakeMetrics>().Instance(_snowflakeMetricsMock.Object));
			container.Register(Component.For<ToggleCheckInterceptor>());
			container.Register(Component.For<PermissionCheckInterceptor>());
			container.Register(Component.For<LogInterceptor>());
			container.Register(Component.For<MetricsInterceptor>());
			container.Register(Component.For<UnhandledExceptionInterceptor>());
			container.Register(Component.For<IMetricsContext>().Instance(_metricsMock.Object));
			container.Register(Component.For<Func<IMetricsContext>>().UsingFactoryMethod(x =>
				new Func<IMetricsContext>(container.Resolve<IMetricsContext>)));
			container.Register(Component.For<IBatchResultCache>().ImplementedBy<BatchResultCache>());
			container.Register(Component.For<IHelper>().Instance(_helperMock.Object));
			container.Register(Component.For<ISqlExecutor>().Instance(_sqlExecutorMock.Object));
			container.Register(Component.For<ISqlRetryPolicy>().Instance(_sqlRetryPolicyMock.Object));
			container.Register(Component.For<IBulkImportService>().ImplementedBy<BulkImportService>());

			_uut = container.Resolve<IBulkImportService>();
		}

		[Test]
		public void ShouldThrowServiceExceptionOnAllEndpointsWhenCommunicationModeTurnsKeplerServicesOff()
		{
			_communicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, IAPICommunicationMode.ForceWebAPI)));

			FluentActions.Invoking(() =>
					_uut.BulkImportImageAsync(Any.Integer(), Any.InstanceOf<SDK.ImportExport.V1.Models.ImageLoadInfo>(),
						Any.Boolean(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.BulkImportNativeAsync(Any.Integer(), Any.InstanceOf<SDK.ImportExport.V1.Models.NativeLoadInfo>(),
						Any.Boolean(), Any.Boolean(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.BulkImportObjectsAsync(Any.Integer(), Any.InstanceOf<SDK.ImportExport.V1.Models.ObjectLoadInfo>(),
						Any.Boolean(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.BulkImportProductionImageAsync(Any.Integer(),
						Any.InstanceOf<SDK.ImportExport.V1.Models.ImageLoadInfo>(), Any.Integer(), Any.Boolean(),
						Any.String()))
				.Should().Throw<ServiceException>()
				.WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.DisposeTempTablesAsync(Any.Integer(), Any.String(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.GenerateImageErrorFilesAsync(Any.Integer(), Any.String(), Any.Boolean(), Any.Integer(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.GenerateNonImageErrorFilesAsync(Any.Integer(), Any.String(), Any.Integer(), Any.Boolean(), Any.Integer(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.HasImportPermissionsAsync(Any.Integer(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.ImageRunHasErrorsAsync(Any.Integer(), Any.String(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");

			FluentActions.Invoking(() =>
					_uut.NativeRunHasErrorsAsync(Any.Integer(), Any.String(), Any.String()))
				.Should().Throw<ServiceException>().WithMessage("IAPI communication mode set to ForceWebAPI. Kepler service disabled.");
		}

		[Test]
		public void ShouldThrowPermissionDeniedExceptionOnAllEndpointsWhenCallerIsNotPermittedDoExecuteCall()
		{
			_communicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, Any.OtherThan(IAPICommunicationMode.ForceWebAPI))));
			var baseServiceContext = Any.InstanceOf<BaseServiceContext>();
			_serviceContextFactoryMock.Setup(x => x.GetBaseServiceContext(It.IsAny<int>()))
				.Returns(baseServiceContext);
			_relativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientImport)).Returns(false);
			_relativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientExport)).Returns(false);

			FluentActions.Invoking(() =>
					_uut.BulkImportImageAsync(Any.Integer(), Any.InstanceOf<SDK.ImportExport.V1.Models.ImageLoadInfo>(),
						Any.Boolean(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.BulkImportNativeAsync(Any.Integer(), Any.InstanceOf<SDK.ImportExport.V1.Models.NativeLoadInfo>(),
						Any.Boolean(), Any.Boolean(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.BulkImportObjectsAsync(Any.Integer(), Any.InstanceOf<SDK.ImportExport.V1.Models.ObjectLoadInfo>(),
						Any.Boolean(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.BulkImportProductionImageAsync(Any.Integer(),
						Any.InstanceOf<SDK.ImportExport.V1.Models.ImageLoadInfo>(), Any.Integer(), Any.Boolean(),
						Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");


			FluentActions.Invoking(() =>
					_uut.DisposeTempTablesAsync(Any.Integer(), Any.String(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.GenerateImageErrorFilesAsync(Any.Integer(), Any.String(), Any.Boolean(), Any.Integer(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.GenerateNonImageErrorFilesAsync(Any.Integer(), Any.String(), Any.Integer(), Any.Boolean(), Any.Integer(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.HasImportPermissionsAsync(Any.Integer(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.ImageRunHasErrorsAsync(Any.Integer(), Any.String(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");

			FluentActions.Invoking(() =>
					_uut.NativeRunHasErrorsAsync(Any.Integer(), Any.String(), Any.String()))
				.Should().Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");
		}
	}
}
