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
		void ExecuteWithPermissionException();
		void ExecuteWithInsufficientAccessControlListPermissions();
		void ExecuteWithBaseException();
		void ExecuteWithBaseExceptionDifferentMessage();
	}

	[Interceptor(typeof(UnhandledExceptionInterceptor))]

	public class UnhandledExceptionInterceptorTestsClass : IUnhandledExceptionInterceptorTestsClass
	{
		public void Execute()
		{
			throw Any.Exception();
		}

		public void ExecuteWithPermissionException()
		{
			throw new Relativity.Core.Exception.Permission("You do not have permission to view this item (ArtifactID=12345678)");
		}

		public void ExecuteWithInsufficientAccessControlListPermissions()
		{
			throw new Relativity.Core.Exception.InsufficientAccessControlListPermissions("Insufficient Permissions! Please ask your Relativity Administrator to allow you import permission.");
		}

		public void ExecuteWithBaseException()
		{
			throw new Relativity.Core.Exception.BaseException("ArtifactID 1234567 does not exist.");
		}

		public void ExecuteWithBaseExceptionDifferentMessage()
		{
			throw new Relativity.Core.Exception.BaseException("Some message");
		}
	}
}
