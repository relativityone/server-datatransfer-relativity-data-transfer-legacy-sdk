// <copyright file="LogInterceptorTests.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Tests.Interceptors.TestClasses;
using Relativity.Services.Interfaces.UserInfo.Models;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Interceptors
{
	/// <summary>
	/// LogInterceptorTests.
	/// </summary>
	public class LogInterceptorTests
	{
		private const int Addend1 = 1;
		private const int Addend2 = 2;
		private const int ExpectedSum = 3;
		private const int ExpectedNumberOfItemsInContext = 6;

		private Mock<IAPILog> _loggerMock;
		private Mock<IAuthenticationMgr> _authenticationManagerMock;
		private ILogInterceptorTestClass _interceptedObject;

		/// <summary>
		/// Gets or sets a value indicating whether functionExecuted.
		/// </summary>
		public static bool FunctionExecuted { get; set; }

		/// <summary>
		/// LogInterceptorTestsSetup.
		/// </summary>
		[SetUp]
		public void LogInterceptorTestsSetup()
		{
			_loggerMock = new Mock<IAPILog>();
			_authenticationManagerMock = new Mock<IAuthenticationMgr>();

			var container = new WindsorContainer();
			container.Register(Component.For<IAPILog>().Instance(_loggerMock.Object));
			container.Register(Component.For<IAuthenticationMgr>().Instance(_authenticationManagerMock.Object));
			container.Register(Component.For<LogInterceptor>());
			container.Register(Component.For<ILogInterceptorTestClass>().ImplementedBy<LogInterceptorTestClass>());

			_interceptedObject = container.Resolve<ILogInterceptorTestClass>();
		}

		/// <summary>
		/// Intercept_ReturnsVoid_WhenCalledAddValues1.
		/// </summary>
		[Test]
		[Category("Unit")]
		public void Intercept_ReturnsVoid_WhenCalledAddValues1()
		{
			// Arrange
			const string NameController = "Controller";
			const string NameEndpointCalled = "EndpointCalled";
			const string NameUserId = "UserId";
			const string NameWorkspaceUserId = "WorkspaceUserId";
			const string NameAddend1 = "a";
			const string NameAddend2 = "b";

			const int ExpectedUserId = 8;
			const int ExpectedWorkspaceUserId = 9;

			_loggerMock.Setup(m => m.LogContextPushProperty(It.IsAny<string>(), It.IsAny<string>()));
			var userInfoMock = new Mock<IUserInfo>();
			userInfoMock.SetupGet(x => x.ArtifactID).Returns(ExpectedUserId);
			userInfoMock.SetupGet(x => x.WorkspaceUserArtifactID).Returns(ExpectedWorkspaceUserId);
			_authenticationManagerMock.SetupGet(x => x.UserInfo).Returns(userInfoMock.Object);

			// Act
			_interceptedObject.AddValues1(Addend1, Addend2);

			// Assert
			_loggerMock.Verify(m => m.LogContextPushProperty(NameController, nameof(LogInterceptorTestClass)), Times.Once);
			_loggerMock.Verify(m => m.LogContextPushProperty(NameEndpointCalled, nameof(LogInterceptorTestClass.AddValues1)), Times.Once);
			_loggerMock.Verify(m => m.LogContextPushProperty(NameUserId, ExpectedUserId), Times.Once);
			_loggerMock.Verify(m => m.LogContextPushProperty(NameWorkspaceUserId, ExpectedWorkspaceUserId), Times.Once);
			_loggerMock.Verify(m => m.LogContextPushProperty(NameAddend1, Addend1.ToString()), Times.Once);
			_loggerMock.Verify(m => m.LogContextPushProperty(NameAddend2, Addend2.ToString()), Times.Once);
		}

		/// <summary>
		/// Intercept_ReturnsInt_WhenCalledAddValues2.
		/// </summary>
		[Test]
		[Category("Unit")]
		public void Intercept_ReturnsInt_WhenCalledAddValues2()
		{
			// Act
			var result = _interceptedObject.AddValues2(Addend1, Addend2);

			// Assert
			Assert.That(result.Equals(ExpectedSum));
		}

		/// <summary>
		/// Intercept_ReturnsTask_WhenCalledAddValues3.
		/// </summary>
		/// <returns>Task.</returns>
		[Test]
		[Category("Unit")]
		public async Task Intercept_ReturnsTask_WhenCalledAddValues3()
		{
			// Act
			var result = await _interceptedObject.AddValues3(Addend1, Addend2);

			// Assert
			Assert.That(result.Equals(ExpectedSum));
		}

		/// <summary>
		/// Intercept_ReturnsTask_WhenCalledAddValues4.
		/// </summary>
		/// <returns>Task.</returns>
		[Test]
		[Category("Unit")]
		public async Task Intercept_ReturnsTask_WhenCalledAddValues4()
		{
			// Act
			await _interceptedObject.AddValues4(Addend1, Addend2, out int result);

			// Assert
			Assert.That(result, Is.EqualTo(ExpectedSum));
		}

		/// <summary>
		/// Intercept_DisposesLogContextProperty_WhenAddValues5Completes.
		/// </summary>
		/// <returns>Task.</returns>
		[Test]
		[Category("Unit")]
		public async Task Intercept_DisposesLogContextProperty_WhenAddValues5Completes()
		{
			// Arrange
			var disposableMock = new Mock<IDisposable>();

			_loggerMock.Setup(m => m.LogContextPushProperty(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(disposableMock.Object);

			disposableMock.Setup(m => m.Dispose())
				.Callback(() =>
				{
					Assert.That(FunctionExecuted, Is.True);
				});

			FunctionExecuted = false;

			// Act
			await _interceptedObject.AddValues5(Addend1, Addend2);

			// Assert
			disposableMock.Verify(m => m.Dispose(), Times.Exactly(ExpectedNumberOfItemsInContext));
		}

		/// <summary>
		/// Intercept_ReturnsTask_WhenCalledAddValues6.
		/// </summary>
		/// <returns>Task.</returns>
		[Test]
		[Category("Unit")]
		public async Task Intercept_ReturnsTask_WhenCalledAddValues6()
		{
			// Arrange
			var disposableMock = new Mock<IDisposable>();

			_loggerMock.Setup(m => m.LogContextPushProperty(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(disposableMock.Object);

			disposableMock.Setup(m => m.Dispose())
				.Callback(() =>
				{
					Assert.That(FunctionExecuted, Is.True);
				});

			FunctionExecuted = false;

			// Act
			await _interceptedObject.AddValues6(Addend1, Addend2);

			// Assert
			disposableMock.Verify(m => m.Dispose(), Times.Exactly(ExpectedNumberOfItemsInContext));
		}

		/// <summary>
		/// Intercept_ReturnsInt_WhenCalledAddValues7.
		/// </summary>
		[Test]
		[Category("Unit")]
		public void Intercept_ReturnsInt_WhenCalledAddValues7()
		{
			// Arrange
			var disposableMock = new Mock<IDisposable>();

			_loggerMock.Setup(m => m.LogContextPushProperty(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(disposableMock.Object);

			disposableMock.Setup(m => m.Dispose())
				.Callback(() =>
				{
					Assert.That(FunctionExecuted, Is.True);
				});

			FunctionExecuted = false;

			// Act
			_interceptedObject.AddValues7(Addend1, Addend2);

			// Assert
			disposableMock.Verify(m => m.Dispose(), Times.Exactly(ExpectedNumberOfItemsInContext));
		}


		/// <summary>
		/// Intercept_SensitiveData.
		/// </summary>
		[Test]
		[Category("Unit")]
		public void Intercept_SensitiveData()
		{
			// Arrange
			string NameAddend1 = "sensitiveData";
			string NameAddend2 = "notSensitiveData";
			string sensitiveData = "value1";
			string notSensitiveData = "value2";

			string hashedSensitiveData = "3c9683017f9e4bf33d0fbedd26bf143fd72de9b9dd145441b75f0604047ea28e";

			// Act
			_interceptedObject.SensitiveData(sensitiveData, notSensitiveData);
				
			// Assert
			_loggerMock.Verify(m => m.LogContextPushProperty(NameAddend1, hashedSensitiveData), Times.Once);
			_loggerMock.Verify(m => m.LogContextPushProperty(NameAddend2, notSensitiveData), Times.Once);

			//Make sure sensitive data has never been logged even under different name
			_loggerMock.Verify(m => m.LogContextPushProperty(It.IsAny<string>(), sensitiveData), Times.Never);
		}


		/// <summary>
		/// Intercept_SensitiveDataWithModel.
		/// </summary>
		[Test]
		[Category("Unit")]
		public void Intercept_SensitiveDataWithModel()
		{
			// Arrange
			string sensitiveData = "value1";

			string hashedSensitiveData = "3c9683017f9e4bf33d0fbedd26bf143fd72de9b9dd145441b75f0604047ea28e";

			var testClass = new SDK.ImportExport.V1.Models.ChoiceInfo
			{
				Name = sensitiveData,
				ArtifactID = 1,
				CodeTypeID = 2,
				Order = 3,
				ParentArtifactID = 4
			};

			// Act
			_interceptedObject.SensitiveDataWithModel(testClass);

			// Assert
			_loggerMock.Verify(m => m.LogContextPushProperty("testClass", It.Is<string>(x => x.Contains(hashedSensitiveData))), Times.Once);

			//Make sure sensitive data has never been logged even under different name
			_loggerMock.Verify(m => m.LogContextPushProperty(It.IsAny<string>(), It.Is<string>(x => x!= null && x.Contains(sensitiveData))), Times.Never);
		}

		[Test]
		public void Intercept_ArrayParameter()
		{
			// Arrange
			var array = new int?[] { 1, 2, 3, null };

			// Act 
			_interceptedObject.Array(array);

			// Assert
			string expectedValue = "1;2;3;;";
			_loggerMock.Verify(m => m.LogContextPushProperty("array", expectedValue), Times.Once);
		}

		[Test]
		public void Intercept_ArrayParameter_WhenNull()
		{
			// Act 
			_interceptedObject.Array(null);

			// Assert
			string expectedValue = "null";
			_loggerMock.Verify(m => m.LogContextPushProperty("array", expectedValue), Times.Once);
		}
	}
}