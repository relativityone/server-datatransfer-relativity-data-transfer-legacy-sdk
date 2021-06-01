// <copyright file="IMetricsInterceptorTestClass.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

using System;
using Castle.Core;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Interceptors.TestClasses
{
	/// <summary> 
	/// IMetricsInterceptorTestClass. 
	/// </summary> 
	public interface IMetricsInterceptorTestClass
	{
		/// <summary> 
		/// Run. 
		/// </summary> 
		void Run();
	}

	/// <inheritdoc /> 
	[Interceptor(typeof(MetricsInterceptor))]
	public class MetricsInterceptorTestClass : IMetricsInterceptorTestClass
	{
		private readonly IMetricsContext _metricsContext;

		/// <summary> 
		/// Initializes a new instance of the <see cref="MetricsInterceptorTestClass"/> class. 
		/// </summary> 
		/// <param name="metricsContext">metricsContext.</param> 
		public MetricsInterceptorTestClass(IMetricsContext metricsContext)
		{
			this._metricsContext = metricsContext;
		}

		/// <summary> 
		/// Gets importJobID. 
		/// </summary> 
		public static Guid ImportJobID => default(Guid);

		/// <inheritdoc /> 
		public void Run()
		{
			this._metricsContext.PushProperty("TestMetric", "TestValue");
		}
	}
}