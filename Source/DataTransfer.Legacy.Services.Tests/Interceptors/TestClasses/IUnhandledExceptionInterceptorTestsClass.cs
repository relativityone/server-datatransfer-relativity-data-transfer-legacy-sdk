// <copyright file="IUnhandledExceptionInterceptorTestsClass.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

using Castle.Core;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Interceptors.TestClasses
{
	public interface IUnhandledExceptionInterceptorTestsClass
	{
		void Execute();
	}

	[Interceptor(typeof(UnhandledExceptionInterceptor))]

	public class UnhandledExceptionInterceptorTestsClass : IUnhandledExceptionInterceptorTestsClass
	{
		public void Execute()
		{
			throw Any.Exception();
		}
	}
}
