namespace DataTransfer.Legacy.PostInstallEventHandler.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using Moq;
	using NUnit.Framework;
	using Polly;
	using Relativity.API;
	using Relativity.Services;
	using Relativity.Services.InstanceSetting;

	[TestFixture]
	public class InstanceSettingsServiceTests
	{
		private IInstanceSettingsService _sut;
		private Mock<IHelper> _helperMock;
		private Mock<IAPILog> _loggerMock;
		private Mock<IRetryPolicyProvider> _retryPolicyProviderMock;

		[SetUp]
		public void InstanceSettingsServiceTestsSetup()
		{
			this._helperMock = new Mock<IHelper>();
			this._loggerMock = new Mock<IAPILog>();
			this._retryPolicyProviderMock = new Mock<IRetryPolicyProvider>();
			this._sut = new InstanceSettingsService(this._loggerMock.Object, this._helperMock.Object, this._retryPolicyProviderMock.Object);
			var mockRetryPolicy = Policy
				.Handle<Exception>()
				.WaitAndRetryAsync(
					0,
					retryAttempt => TimeSpan.FromSeconds(0));
			this._retryPolicyProviderMock.Setup(m => m.GetAsyncRetryPolicy(It.IsAny<string>())).Returns(mockRetryPolicy);
		}

		[Test]
		[Category("Unit")]
		public async Task CreateInstanceSettingsTextType_ReturnSuccess_WhenInstanceAlreadyExists()
		{
			// Arrange
			this._loggerMock.ResetCalls();
			string agentName = "Paolo";
			string sectionName = "importini";
			string value = "sample value";
			string desc = "sample description";
			Mock<IInstanceSettingManager> instanceSettingManagerMock = new Mock<IInstanceSettingManager>();
			this._helperMock.Setup(m => m.GetServicesManager().CreateProxy<IInstanceSettingManager>(It.IsAny<ExecutionIdentity>())).Returns(instanceSettingManagerMock.Object);
			instanceSettingManagerMock.Setup(m => m.QueryAsync(It.IsAny<Query>())).ReturnsAsync(new InstanceSettingQueryResultSet() { Success = true, Results = new List<Result<InstanceSetting>>() { new Result<InstanceSetting>() { Artifact = new InstanceSetting() { ArtifactID = 1 } } } });
			instanceSettingManagerMock.Setup(m => m.UpdateSingleAsync(It.IsAny<InstanceSetting>())).Returns(Task.FromResult(0));

			// Act
			bool result = await this._sut.CreateInstanceSettingsTextType(agentName, sectionName, value, desc).ConfigureAwait(false);

			// Assert
			instanceSettingManagerMock.Verify(x => x.QueryAsync(It.IsAny<Query>()), Times.Once);
			instanceSettingManagerMock.Verify(x => x.CreateSingleAsync(It.IsAny<InstanceSetting>()), Times.Never);
			Assert.That(result, Is.EqualTo(true));
		}

		[Test]
		[Category("Unit")]
		public async Task CreateInstanceSettingsTextType_ReturnSuccess_WhenInstanceSettingsCreated()
		{
			// Arrange
			this._loggerMock.ResetCalls();
			string agentName = "Paolo";
			string sectionName = "importini";
			string value = "sample value";
			string desc = "sample description";
			Mock<IInstanceSettingManager> instanceSettingManagerMock = new Mock<IInstanceSettingManager>();
			this._helperMock.Setup(m => m.GetServicesManager().CreateProxy<IInstanceSettingManager>(It.IsAny<ExecutionIdentity>())).Returns(instanceSettingManagerMock.Object);
			instanceSettingManagerMock.Setup(m => m.QueryAsync(It.IsAny<Query>())).ReturnsAsync(new InstanceSettingQueryResultSet() { Success = true, Results = new List<Result<InstanceSetting>>() });
			instanceSettingManagerMock.Setup(m => m.CreateSingleAsync(It.IsAny<InstanceSetting>())).ReturnsAsync(1);

			// Act
			bool result = await this._sut.CreateInstanceSettingsTextType(agentName, sectionName, value, desc).ConfigureAwait(false);

			// Assert
			instanceSettingManagerMock.Verify(x => x.QueryAsync(It.IsAny<Query>()), Times.Once);
			instanceSettingManagerMock.Verify(x => x.CreateSingleAsync(It.IsAny<InstanceSetting>()), Times.Once);
			_loggerMock.Verify(x => x.LogInformation(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<object>()), Times.Once);
			Assert.That(result, Is.EqualTo(true));
		}

		[Test]
		[Category("Unit")]
		public void CreateInstanceSettingsTextType_ThrowException_WhenCreatingInstanceSettingsThrowException()
		{
			// Arrange
			this._loggerMock.ResetCalls();
			string agentName = "Paolo";
			string sectionName = "importini";
			string value = "sample value";
			string desc = "sample description";
			Mock<IInstanceSettingManager> instanceSettingManagerMock = new Mock<IInstanceSettingManager>();
			this._helperMock.Setup(m => m.GetServicesManager().CreateProxy<IInstanceSettingManager>(It.IsAny<ExecutionIdentity>())).Returns(instanceSettingManagerMock.Object);
			instanceSettingManagerMock.Setup(m => m.QueryAsync(It.IsAny<Query>())).ReturnsAsync(new InstanceSettingQueryResultSet() { Success = true, Results = new List<Result<InstanceSetting>>() });
			instanceSettingManagerMock.Setup(m => m.CreateSingleAsync(It.IsAny<InstanceSetting>())).Throws<Exception>();

			// Act && Assert
			Assert.ThrowsAsync<Exception>(() => this._sut.CreateInstanceSettingsTextType(agentName, sectionName, value, desc));
			instanceSettingManagerMock.Verify(x => x.QueryAsync(It.IsAny<Query>()), Times.Once);
			instanceSettingManagerMock.Verify(x => x.CreateSingleAsync(It.IsAny<InstanceSetting>()), Times.Once);
		}
	}
}