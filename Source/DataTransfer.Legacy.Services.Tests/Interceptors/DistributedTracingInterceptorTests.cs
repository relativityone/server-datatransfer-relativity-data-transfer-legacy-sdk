// <copyright file="DistributedTracingInterceptorTests.cs" company="Relativity ODA LLC"> 
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
using Relativity.DataTransfer.Legacy.Services.Tests.Interceptors.TestClasses;
using Relativity.DataTransfer.Legacy.Services.Observability;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Interceptors
{
	/// <summary> 
	/// DistributedTracingInterceptorTests. 
	/// </summary> 
	[TestFixture]
	public class DistributedTracingInterceptorTests
	{
		private Mock<IAPILog> _loggerMock;
		private Mock<ITraceGenerator> _traceGeneratorMock;

		/// <summary> 
		/// MetricsInterceptorTestsSetup. 
		/// </summary> 
		[SetUp]
		public void DistributedTracingInterceptorSetup()
		{
			_loggerMock = new Mock<IAPILog>();
			_traceGeneratorMock = new Mock<ITraceGenerator>();
			
			var store = new DefaultConfigurationStore();
			var container = new WindsorContainer(store);
			container.Register(Component.For<MetricsInterceptor>());
			container.Register(Component.For<IMetricsInterceptorTestClass>().ImplementedBy<MetricsInterceptorTestClass>());
			container.Register(Component.For<IAPILog>().Instance(_loggerMock.Object));
			//container.Register(Component.For<IMetricsContext>().Instance(_metricsContextMock.Object));
			//container.Register(Component.For<IRelEyeMetricsService>().Instance(_relEyeMetricsServiceMock.Object).LifestyleTransient());
			//container.Register(Component.For<IEventsBuilder>().Instance(_eventsBuilderMock.Object).LifestyleTransient());
			container.AddFacility<TypedFactoryFacility>();

			//_interceptedObject = container.Resolve<IMetricsInterceptorTestClass>();
		}

		/// <summary> 
		/// InterceptRunMethod. 
		/// </summary> 
		[Test]
		public void InterceptRunMethod()
		{
			// Act 
			//_interceptedObject.Run();

			// Assert 
			//_metricsContextMock.Verify(m => m.PushProperty($"TargetType", It.IsAny<object>()));
			//_metricsContextMock.Verify(m => m.PushProperty($"Method", It.IsAny<object>()));
			//_metricsContextMock.Verify(m => m.PushProperty($"ElapsedMilliseconds", It.IsAny<object>()));
			//_metricsContextMock.Verify(m => m.Publish());

			//_eventsBuilderMock.Verify(m => m.BuildGeneralStatisticsEvent("some runID", 12345));
			//_relEyeMetricsServiceMock.Verify(m => m.PublishEvent(It.IsAny<EventGeneralStatistics>()));
		}
	}
}