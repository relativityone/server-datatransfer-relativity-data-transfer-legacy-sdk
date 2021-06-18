using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests
{
	[TestFixture]
	public class IAPICommunicationModeServiceTests
	{
		private Mock<ICommunicationModeStorage> _connectionModeStorage;
		private IIAPICommunicationModeService _uut;
		private Mock<IAPILog> _logger;

		[SetUp]
		public void SetUp()
		{
			var serviceContextFactory = new Mock<IServiceContextFactory>();
			_connectionModeStorage = new Mock<ICommunicationModeStorage>();
			_logger = new Mock<IAPILog>();
			_uut = new IAPICommunicationModeService(serviceContextFactory.Object, _logger.Object, _connectionModeStorage.Object);
		}

		[Test]
		public async Task ShouldReturnWebAPIModeAndLogWhenReadingModeThrowsException()
		{
			_connectionModeStorage.Setup(x => x.TryGetModeAsync())
				.Throws(Any.Exception());
			_logger.Setup(x => x.LogWarning(It.IsAny<string>()));
			var storageKey = Any.String();
			_connectionModeStorage.Setup(x => x.GetStorageKey())
				.Returns(storageKey);

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(IAPICommunicationMode.WebAPI);
			_logger.Verify(x => x.LogWarning($"'{storageKey}' toggle not found. WebAPI IAPI communication mode will be used."), Times.Once);
		}

		[Test]
		public async Task ShouldReturnWebAPIModeAndLogWhenReadingModeReturnsNoMode()
		{
			_connectionModeStorage.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((false, Any.ValueOf<IAPICommunicationMode>())));
			_logger.Setup(x => x.LogWarning(It.IsAny<string>()));
			var storageKey = Any.String();
			_connectionModeStorage.Setup(x => x.GetStorageKey())
				.Returns(storageKey);

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(IAPICommunicationMode.WebAPI);
			_logger.Verify(x => x.LogWarning($"Invalid IAPI communication mode in '{storageKey}' toggle. WebAPI IAPI communication mode will be used."), Times.Once);
		}

		[Test]
		public async Task ShouldReturnModeModeAndLogWhenReadingModeReturnsMode()
		{
			var mode = Any.ValueOf<IAPICommunicationMode>();
			_connectionModeStorage.Setup(x => x.TryGetModeAsync())
				.Returns(Task.FromResult((true, mode)));
			_logger.Setup(x => x.LogWarning(It.IsAny<string>()));
			var storageKey = Any.String();

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(mode);
		}
	}
}
