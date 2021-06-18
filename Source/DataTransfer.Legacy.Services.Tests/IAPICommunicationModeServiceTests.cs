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
		private Mock<IInstanceSettingsBundle> _instanceSettingsBundle;
		private IIAPICommunicationModeService _uut;
		private Mock<IAPILog> _logger;

		[SetUp]
		public void SetUp()
		{
			var methodRunner = new BypassMethodRunner();
			var serviceContextFactory = new Mock<IServiceContextFactory>();
			_instanceSettingsBundle = new Mock<IInstanceSettingsBundle>();
			_logger = new Mock<IAPILog>();
			_uut = new IAPICommunicationModeService(methodRunner, serviceContextFactory.Object, _instanceSettingsBundle.Object, _logger.Object);
		}

		[Test]
		public async Task ShouldReturnWebAPIModeAndLogWhenReadingInstanceSettingThrowsException()
		{
			_instanceSettingsBundle.Setup(x => x.GetStringAsync("DataTransfer.Legacy", "IAPICommunicationMode"))
				.Throws(Any.Exception());
			_logger.Setup(x => x.LogWarning(It.IsAny<string>()));

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(IAPICommunicationMode.WebAPI);
			_logger.Verify(x => x.LogWarning("'DataTransfer.Legacy.IAPICommunicationMode' setting not found. WebAPI IAPI communication mode will be used."), Times.Once);
		}

		[TestCase("webapi", IAPICommunicationMode.WebAPI)]
		[TestCase("WebAPI", IAPICommunicationMode.WebAPI)]
		[TestCase("kepler", IAPICommunicationMode.Kepler)]
		[TestCase("Kepler", IAPICommunicationMode.Kepler)]
		[TestCase("forcewebapi", IAPICommunicationMode.ForceWebAPI)]
		[TestCase("ForceWebAPI", IAPICommunicationMode.ForceWebAPI)]
		[TestCase("forcekepler", IAPICommunicationMode.ForceKepler)]
		[TestCase("ForceKepler", IAPICommunicationMode.ForceKepler)]
		public async Task ShouldReturnCorrectModeWhenReadingInstanceSettingReturnsValidValue(string settingValue, IAPICommunicationMode expectedMode)
		{
			_instanceSettingsBundle.Setup(x => x.GetStringAsync("DataTransfer.Legacy", "IAPICommunicationMode"))
				.Returns(Task.FromResult(settingValue));

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(expectedMode);
		}

		[Test]
		public async Task ShouldReturnWebAPIModeAndLogWhenReadingInstanceSettingReturnsUnrecognizedMode()
		{
			var settingValue = Any.OtherThan("webapi", "kepler", "forcewebapi", "forcekepler");
			_instanceSettingsBundle.Setup(x => x.GetStringAsync("DataTransfer.Legacy", "IAPICommunicationMode"))
				.Returns(Task.FromResult(settingValue));
			_logger.Setup(x => x.LogWarning(It.IsAny<string>()));

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(IAPICommunicationMode.WebAPI);
			_logger.Verify(x => x.LogWarning($"Invalid IAPI communication mode in 'DataTransfer.Legacy.IAPICommunicationMode' setting. WebAPI IAPI communication mode will be used."), Times.Once);
		}
	}
}
