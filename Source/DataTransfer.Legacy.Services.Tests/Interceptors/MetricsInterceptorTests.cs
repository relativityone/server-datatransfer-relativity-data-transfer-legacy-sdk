﻿// <copyright file="MetricsInterceptorTests.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using Relativity.DataTransfer.Legacy.Services.Tests.Interceptors.TestClasses;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Interceptors
{
	/// <summary> 
	/// MetricsInterceptorTests. 
	/// </summary> 
	[TestFixture]
	public class MetricsInterceptorTests
	{
		private Mock<IAPILog> _loggerMock;
		private Mock<IMetricsContext> _metricsContextMock;
		private IMetricsInterceptorTestClass _interceptedObject;

		/// <summary> 
		/// MetricsInterceptorTestsSetup. 
		/// </summary> 
		[SetUp]
		public void MetricsInterceptorTestsSetup()
		{
			_loggerMock = new Mock<IAPILog>();
			_metricsContextMock = new Mock<IMetricsContext>();

			int callOrder = 1;
			_metricsContextMock.Setup(x => x.PushProperty("TestMetric", "TestValue")).Callback(() => Assert.That(callOrder++, Is.EqualTo(1)));
			_metricsContextMock.Setup(x => x.PushProperty("CallDuration", It.IsAny<long>())).Callback(() => Assert.That(callOrder++, Is.EqualTo(2)));

			var store = new DefaultConfigurationStore();
			var container = new WindsorContainer(store);
			container.Register(Component.For<MetricsInterceptor>());
			container.Register(Component.For<IMetricsInterceptorTestClass>().ImplementedBy<MetricsInterceptorTestClass>());
			container.Register(Component.For<IAPILog>().Instance(_loggerMock.Object));
			container.Register(Component.For<IMetricsContext>().Instance(_metricsContextMock.Object));
			container.AddFacility<TypedFactoryFacility>();

			_interceptedObject = container.Resolve<IMetricsInterceptorTestClass>();
		}

		/// <summary> 
		/// InterceptRunMethod. 
		/// </summary> 
		[Test]
		public void InterceptRunMethod()
		{
			// Act 
			_interceptedObject.Run();

			// Assert 
			_metricsContextMock.Verify(m => m.PushProperty($"TargetType", It.IsAny<object>()));
			_metricsContextMock.Verify(m => m.PushProperty($"Method", It.IsAny<object>()));
			_metricsContextMock.Verify(m => m.PushProperty($"ElapsedMilliseconds", It.IsAny<object>()));

			_metricsContextMock.Verify(m => m.Publish());
		}
	}
}