using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests
{
	[TestFixture]
	public class CommunicationModeInstanceSettingStorageTests
	{
		private Mock<IInstanceSettingsBundle> _instanceSettingsBundle;
		private ICommunicationModeStorage _uut;

		[SetUp]
		public void SetUp()
		{
			_instanceSettingsBundle = new Mock<IInstanceSettingsBundle>();
			_uut = new CommunicationModeInstanceSettingStorage(_instanceSettingsBundle.Object);
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

			var (success, result) = await _uut.TryGetModeAsync();

			success.Should().BeTrue();
			result.Should().Be(expectedMode);
		}

		[Test]
		public async Task ShouldReturnFailWithNoModeWhenReadingInstanceSettingReturnsUnrecognizedMode()
		{
			var settingValue = Any.OtherThan("webapi", "kepler", "forcewebapi", "forcekepler");
			_instanceSettingsBundle.Setup(x => x.GetStringAsync("DataTransfer.Legacy", "IAPICommunicationMode"))
				.Returns(Task.FromResult(settingValue));

			var (success, result) = await _uut.TryGetModeAsync();

			success.Should().BeFalse();
			result.Should().Be(IAPICommunicationMode.WebAPI);
		}

		[Test]
		public void ShouldReturnInstanceSettingNameWhenStorageKeyIsRetrieved()
		{
			var result = _uut.GetStorageKey();

			result.Should().Be("DataTransfer.Legacy.IAPICommunicationMode");
		}
	}
}
