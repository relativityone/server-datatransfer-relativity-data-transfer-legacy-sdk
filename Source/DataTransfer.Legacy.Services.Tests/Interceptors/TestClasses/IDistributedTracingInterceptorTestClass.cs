using Relativity.DataTransfer.Legacy.Services.Interceptors;
using System;
using Castle.Core;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.Observability;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Interceptors.TestClasses
{
	/// <summary> 
	/// IDistributedTracingInterceptorTestClass. 
	/// </summary> 
	public interface IDistributedTracingInterceptorTestClass
	{
		void Run();

		void RunWithID(string correlationID);
	}

	/// <inheritdoc /> 
	[Interceptor(typeof(DistributedTracingInterceptor))]
	public class DistributedTracingInterceptorTestClass : IDistributedTracingInterceptorTestClass
	{
		private readonly IAPILog _logger;
		private readonly ITraceGenerator _traceGenerator;

		/// <summary> 
		/// Initializes a new instance of the <see cref="DistributedTracingInterceptorTestClass"/> class. 
		/// </summary> 
		/// <param name="metricsContext">metricsContext.</param> 
		public DistributedTracingInterceptorTestClass(IAPILog logger, ITraceGenerator traceGenerator)
		{
			this._logger = logger;
			this._traceGenerator = traceGenerator;
		}

		/// <summary> 
		/// Gets importJobID. 
		/// </summary> 
		public static Guid ImportJobID => default(Guid);

		public void Run()
		{

		}

		/// <inheritdoc /> 
		public void RunWithID(string correlationID)
		{
			
		}
	}
}
