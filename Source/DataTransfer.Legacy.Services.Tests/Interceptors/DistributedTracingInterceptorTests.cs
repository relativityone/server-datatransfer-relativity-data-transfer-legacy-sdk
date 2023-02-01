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
using System.Diagnostics;
using System.Collections.Generic;
using System;

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
		private IDistributedTracingInterceptorTestClass _interceptedObject;

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
			container.Register(Component.For<DistributedTracingInterceptor>());
			container.Register(Component.For<IDistributedTracingInterceptorTestClass>().ImplementedBy<DistributedTracingInterceptorTestClass>());
			container.Register(Component.For<IAPILog>().Instance(_loggerMock.Object));
			container.Register(Component.For<ITraceGenerator>().Instance(_traceGeneratorMock.Object));
			container.AddFacility<TypedFactoryFacility>();

			_interceptedObject = container.Resolve<IDistributedTracingInterceptorTestClass>();
		}

		[Test]
		public void InterceptRunMethod()
		{
			// Act 
			_interceptedObject.Run();

			// Assert 
			_traceGeneratorMock.Verify(m => m.StartActivity(It.IsAny<string>(), It.IsAny<ActivityKind>(), It.IsAny<ActivityContext>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>(), It.IsAny<IEnumerable<ActivityLink>>(), It.IsAny<DateTimeOffset>()), Times.Never);
		}

		[Test]
		public void InterceptRunWithIdMethod()
		{
			// Act 
			_interceptedObject.RunWithID("test");

			// Assert 
			_traceGeneratorMock.Verify(m => m.StartActivity(It.IsAny<string>(), It.IsAny<ActivityKind>(), It.IsAny<ActivityContext>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>(), It.IsAny<IEnumerable<ActivityLink>>(), It.IsAny<DateTimeOffset>()), Times.Never);
		}

		[Test]
		public void InterceptRunWithSerializedActivityContextAsIdMethod()
		{
			// Act 
			_interceptedObject.RunWithID("00-64200095b15d3185f702523c236c6920-9e017cf1e496af28-01");

			// Assert 
			_traceGeneratorMock.Verify(m => m.StartActivity(It.IsAny<string>(), It.IsAny<ActivityKind>(), It.IsAny<ActivityContext>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>(), It.IsAny<IEnumerable<ActivityLink>>(), It.IsAny<DateTimeOffset>()), Times.Once);
		}

		[Test]
		public void InterceptRunShallLogError()
		{
			// Arrange
			_traceGeneratorMock.Setup(m => m.StartActivity(It.IsAny<string>(), It.IsAny<ActivityKind>(), It.IsAny<ActivityContext>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>(), It.IsAny<IEnumerable<ActivityLink>>(), It.IsAny<DateTimeOffset>())).Throws(new Exception("test"));
			
			// Act 
			_interceptedObject.RunWithID("00-64200095b15d3185f702523c236c6920-9e017cf1e496af28-01");

			// Assert 
			_traceGeneratorMock.Verify(m => m.StartActivity(It.IsAny<string>(), It.IsAny<ActivityKind>(), It.IsAny<ActivityContext>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>(), It.IsAny<IEnumerable<ActivityLink>>(), It.IsAny<DateTimeOffset>()), Times.Once);
			_loggerMock.Verify(m => m.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));
		}

		[Test]
		public void InterceptRunWithSerializedActivityContextAsIdsMethod()
		{
			// Act 
			_interceptedObject.RunWithIDs(1234, "test", "00-64200095b15d3185f702523c236c6920-9e017cf1e496af28-01");

			// Assert 
			_traceGeneratorMock.Verify(m => m.StartActivity(It.IsAny<string>(), It.IsAny<ActivityKind>(), It.IsAny<ActivityContext>(), It.IsAny<IEnumerable<KeyValuePair<string, object>>>(), It.IsAny<IEnumerable<ActivityLink>>(), It.IsAny<DateTimeOffset>()), Times.Once);
		}
	}
}