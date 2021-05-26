using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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
		private IInstanceSettingsBundle _instanceSettingsBundle;
		private IIAPICommunicationModeService _uut;

		[SetUp]
		public void SetUp()
		{
			var methodRunner = new BypassMethodRunner();
			var serviceContextFactory = Substitute.For<IServiceContextFactory>();
			_instanceSettingsBundle = Substitute.For<IInstanceSettingsBundle>();
			_uut = new IAPICommunicationModeService(methodRunner, serviceContextFactory, _instanceSettingsBundle);
		}

		[Test]
		public async Task ShouldReturnWebAPIModeWhenReadingInstanceSettingThrowsException()
		{
			_instanceSettingsBundle.GetStringAsync("DataTransfer.Legacy", "IAPICommunicationMode")
				.Throws(Any.Exception());

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(IAPICommunicationMode.WebAPI);
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
			_instanceSettingsBundle.GetStringAsync("DataTransfer.Legacy", "IAPICommunicationMode")
				.Returns(settingValue);

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(expectedMode);
		}

		[Test]
		public async Task ShouldReturnWebAPIModeWhenReadingInstanceSettingReturnsUnrecognizedMode()
		{
			var settingValue = Any.OtherThan("webapi", "kepler", "forcewebapi", "forcekepler");
			_instanceSettingsBundle.GetStringAsync("DataTransfer.Legacy", "IAPICommunicationMode")
				.Returns(settingValue);

			var result = await _uut.GetIAPICommunicationModeAsync(Any.String());

			result.Should().Be(IAPICommunicationMode.WebAPI);
		}
	}
}
