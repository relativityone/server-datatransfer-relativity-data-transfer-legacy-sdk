using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Core;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests
{
	[TestFixture]
	public class IAPICommunicationModeServiceTests : ServicesBaseTests
	{
		private IIAPICommunicationModeService _uut;

		[SetUp]
		public void SetUp()
		{
			_uut = Container.Resolve<IIAPICommunicationModeService>();
		}

		[Test]
		public async Task ShouldReturnWebAPIModeAndLogWhenReadingModeThrowsException()
		{
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, Any.OtherThan(IAPICommunicationMode.ForceWebAPI))));
			var baseServiceContext = Any.InstanceOf<BaseServiceContext>();
			ServiceContextFactoryMock.Setup(x => x.GetBaseServiceContext(It.IsAny<int>()))
				.Returns(baseServiceContext);
			RelativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientImport)).Returns(true);
			RelativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientExport)).Returns(true);
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Throws(Any.Exception());
			LoggerMock.Setup(x => x.LogWarning(It.IsAny<string>()));
			var storageKey = Any.String();
			CommunicationModeStorageMock.Setup(x => x.GetStorageKey())
				.Returns(storageKey);

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(IAPICommunicationMode.WebAPI);
			LoggerMock.Verify(x => x.LogWarning($"'{storageKey}' toggle not found. WebAPI IAPI communication mode will be used."), Times.Once);
		}

		[Test]
		public async Task ShouldReturnWebAPIModeAndLogWhenReadingModeReturnsNoMode()
		{
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, Any.OtherThan(IAPICommunicationMode.ForceWebAPI))));
			var baseServiceContext = Any.InstanceOf<BaseServiceContext>();
			ServiceContextFactoryMock.Setup(x => x.GetBaseServiceContext(It.IsAny<int>()))
				.Returns(baseServiceContext);
			RelativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientImport)).Returns(true);
			RelativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientExport)).Returns(true);
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((false, Any.ValueOf<IAPICommunicationMode>())));
			LoggerMock.Setup(x => x.LogWarning(It.IsAny<string>()));
			var storageKey = Any.String();
			CommunicationModeStorageMock.Setup(x => x.GetStorageKey())
				.Returns(storageKey);

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(IAPICommunicationMode.WebAPI);
			LoggerMock.Verify(x => x.LogWarning($"Invalid IAPI communication mode in '{storageKey}' toggle. WebAPI IAPI communication mode will be used."), Times.Once);
		}

		[Test]
		public async Task ShouldReturnModeModeAndLogWhenReadingModeReturnsModeAndToggleReturnsModeOtherThanForceWebApi()
		{
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, Any.OtherThan(IAPICommunicationMode.ForceWebAPI))));
			var baseServiceContext = Any.InstanceOf<BaseServiceContext>();
			ServiceContextFactoryMock.Setup(x => x.GetBaseServiceContext(It.IsAny<int>()))
				.Returns(baseServiceContext);
			RelativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientImport)).Returns(true);
			RelativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientExport)).Returns(true);
			var mode = Any.ValueOf<IAPICommunicationMode>();
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, mode)));
			LoggerMock.Setup(x => x.LogWarning(It.IsAny<string>()));

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(mode);
		}

		[Test]
		public async Task ShouldReturnModeModeAndLogWhenReadingModeReturnsModeAndToggleReturnsForceWebApi()
		{
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, IAPICommunicationMode.ForceWebAPI)));
			var baseServiceContext = Any.InstanceOf<BaseServiceContext>();
			ServiceContextFactoryMock.Setup(x => x.GetBaseServiceContext(It.IsAny<int>()))
				.Returns(baseServiceContext);
			RelativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientImport)).Returns(true);
			RelativityPermissionHelperMock.Setup(x =>
				x.HasAdminOperationPermission(baseServiceContext, Permission.AllowDesktopClientExport)).Returns(true);
			var mode = Any.ValueOf<IAPICommunicationMode>();
			CommunicationModeStorageMock.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, mode)));
			LoggerMock.Setup(x => x.LogWarning(It.IsAny<string>()));

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(mode);
		}
	}
}
