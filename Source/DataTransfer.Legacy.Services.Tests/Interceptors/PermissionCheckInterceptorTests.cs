﻿using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Core;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Tests.Interceptors.TestClasses;
using Relativity.Services.Objects.Exceptions;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Interceptors
{
	using Relativity.Core.Exception;
	using Relativity.Services.Exceptions;
	using Permission = Relativity.Core.Permission;

	[TestFixture]
	public class PermissionCheckInterceptorTests
	{
		private Mock<IAPILog> _loggerMock;
		private Mock<IServiceContextFactory> _serviceContextFactoryMock;
		private IPermissionCheckInterceptorTestClass _interceptedObject;
		private Mock<IRelativityPermissionHelper> _relativityPermissionHelper;

		[SetUp]
		public void SetUp()
		{
			_loggerMock = new Mock<IAPILog>();
			_serviceContextFactoryMock = new Mock<IServiceContextFactory>();
			_relativityPermissionHelper = new Mock<IRelativityPermissionHelper>();

			var container = new WindsorContainer();
			container.Register(Component.For<IAPILog>().Instance(_loggerMock.Object));
			container.Register(Component.For<IServiceContextFactory>().Instance(_serviceContextFactoryMock.Object));
			container.Register(Component.For<IRelativityPermissionHelper>().Instance(_relativityPermissionHelper.Object));
			container.Register(Component.For<PermissionCheckInterceptor>());
			container.Register(Component.For<IPermissionCheckInterceptorTestClass>().ImplementedBy<PermissionCheckInterceptorTestClass>());

			_interceptedObject = container.Resolve<IPermissionCheckInterceptorTestClass>();
		}

		[Test]
		public void ShouldNotCheckPermissionsWhenRunMethodWithoutWorkspaceIdArgument()
		{
			_interceptedObject.Run();

			_serviceContextFactoryMock.Verify(x => x.GetBaseServiceContext(It.IsAny<int>()), Times.Never);
		}

		[Test]
		public void ShouldNotCheckPermissionsWhenRunMethodWithWorkspaceIdArgumentSetToNullRegardlessArgumentCasing()
		{
			_interceptedObject.RunWithWorkspaceUnrecognizedCase(null);
			_interceptedObject.RunWithWorkspaceCamelCase(null);
			_interceptedObject.RunWithWorkspaceLowerCase(null);
			_interceptedObject.RunWithWorkspaceRelativityCase(null);

			_serviceContextFactoryMock.Verify(x => x.GetBaseServiceContext(It.IsAny<int>()), Times.Never);
		}

		[Test]
		public void ShouldCheckImportAndExportPermissionsAndNotThrowWhenRunMethodWithWorkspaceIdArgumentSetToAnyIntAndImportAndExportAreAllowedForRecognizedArgCasings()
		{
			var workspaceId = Any.Integer();

			var serviceContext = Any.InstanceOf<BaseServiceContext>();
			_serviceContextFactoryMock.Setup(x => x.GetBaseServiceContext(workspaceId)).Returns(serviceContext);
			_relativityPermissionHelper
				.Setup(x => x.HasAdminOperationPermission(serviceContext, Permission.AllowDesktopClientImport))
				.Returns(true);
			_relativityPermissionHelper
				.Setup(x => x.HasAdminOperationPermission(serviceContext, Permission.AllowDesktopClientExport))
				.Returns(true);

			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceCamelCase(workspaceId)).Should().NotThrow<Exception>();
			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceLowerCase(workspaceId)).Should().NotThrow<Exception>();
			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceRelativityCase(workspaceId)).Should().NotThrow<Exception>();
		}

		[Test]
		public void ShouldCheckImportAndExportPermissionsAndNotThrowWhenRunMethodWithWorkspaceIdArgumentSetToAnyIntAndOnlyImportIsAllowedForRecognizedArgCasings()
		{
			var workspaceId = Any.Integer();

			var serviceContext = Any.InstanceOf<BaseServiceContext>();
			_serviceContextFactoryMock.Setup(x => x.GetBaseServiceContext(workspaceId)).Returns(serviceContext);
			_relativityPermissionHelper
				.Setup(x => x.HasAdminOperationPermission(serviceContext, Permission.AllowDesktopClientImport))
				.Returns(true);
			_relativityPermissionHelper
				.Setup(x => x.HasAdminOperationPermission(serviceContext, Permission.AllowDesktopClientExport))
				.Returns(false);

			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceCamelCase(workspaceId)).Should().NotThrow<Exception>();
			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceLowerCase(workspaceId)).Should().NotThrow<Exception>();
			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceRelativityCase(workspaceId)).Should().NotThrow<Exception>();
		}

		[Test]
		public void ShouldCheckImportAndExportPermissionsAndNotThrowWhenRunMethodWithWorkspaceIdArgumentSetToAnyIntAndOnlyExportIsAllowedForRecognizedArgCasings()
		{
			var workspaceId = Any.Integer();

			var serviceContext = Any.InstanceOf<BaseServiceContext>();
			_serviceContextFactoryMock.Setup(x => x.GetBaseServiceContext(workspaceId)).Returns(serviceContext);
			_relativityPermissionHelper
				.Setup(x => x.HasAdminOperationPermission(serviceContext, Permission.AllowDesktopClientImport))
				.Returns(false);
			_relativityPermissionHelper
				.Setup(x => x.HasAdminOperationPermission(serviceContext, Permission.AllowDesktopClientExport))
				.Returns(true);

			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceCamelCase(workspaceId)).Should().NotThrow<Exception>();
			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceLowerCase(workspaceId)).Should().NotThrow<Exception>();
			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceRelativityCase(workspaceId)).Should().NotThrow<Exception>();
		}

		[Test]
		public void ShouldThrowPermissionDeniedExceptionWhenRunMethodWithWorkspaceIdArgumentSetToAnyIntAndImportAndExportAreNotAllowedForRecognizedArgCasings()
		{
			var workspaceId = Any.Integer();

			var serviceContext = Any.InstanceOf<BaseServiceContext>();
			_serviceContextFactoryMock.Setup(x => x.GetBaseServiceContext(workspaceId)).Returns(serviceContext);
			_relativityPermissionHelper
				.Setup(x => x.HasAdminOperationPermission(serviceContext, Permission.AllowDesktopClientImport))
				.Returns(false);
			_relativityPermissionHelper
				.Setup(x => x.HasAdminOperationPermission(serviceContext, Permission.AllowDesktopClientExport))
				.Returns(false);

			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceCamelCase(workspaceId)).Should()
				.Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");
			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceLowerCase(workspaceId)).Should()
				.Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");
			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceRelativityCase(workspaceId)).Should()
				.Throw<PermissionDeniedException>().WithMessage("User does not have permissions to use WebAPI Kepler replacement");
		}

		[Test(Description = "This tests InterceptorBase error handling, in case of Core.Exception.Permission should rethrow as PermissionDeniedException")]
		public void ShouldThrowPermissionDeniedExceptionWhenPermissionHelperThrowPermissionExceptionToBeHandled()
		{
			var workspaceId = Any.Integer();

			var serviceContext = Any.InstanceOf<BaseServiceContext>();
			_serviceContextFactoryMock.Setup(x => x.GetBaseServiceContext(workspaceId)).Returns(serviceContext);
			_relativityPermissionHelper
				.Setup(x => x.HasAdminOperationPermission(serviceContext, Permission.AllowDesktopClientImport))
				.Throws(new Core.Exception.Permission("Logged in user ID 10240454 does not have access to workspace ID 1018995.  If the logged in user is a member of the system administrators group, please ensure that the logged in user is added to any other group with access to this workspace."));

			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceCamelCase(workspaceId)).Should()
				.Throw<PermissionDeniedException>().WithMessage($"Error during interceptor action {nameof(PermissionCheckInterceptor)} for {nameof(PermissionCheckInterceptorTestClass)}.{nameof(PermissionCheckInterceptorTestClass.RunWithWorkspaceCamelCase)} InnerExceptionType: Relativity.Core.Exception.Permission, InnerExceptionMessage: Logged in user ID 10240454 does not have access to workspace ID 1018995.  If the logged in user is a member of the system administrators group, please ensure that the logged in user is added to any other group with access to this workspace.");
			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceLowerCase(workspaceId)).Should()
                .Throw<PermissionDeniedException>().WithMessage($"Error during interceptor action {nameof(PermissionCheckInterceptor)} for {nameof(PermissionCheckInterceptorTestClass)}.{nameof(PermissionCheckInterceptorTestClass.RunWithWorkspaceLowerCase)} InnerExceptionType: Relativity.Core.Exception.Permission, InnerExceptionMessage: Logged in user ID 10240454 does not have access to workspace ID 1018995.  If the logged in user is a member of the system administrators group, please ensure that the logged in user is added to any other group with access to this workspace.");
			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceRelativityCase(workspaceId)).Should()
                .Throw<PermissionDeniedException>().WithMessage($"Error during interceptor action {nameof(PermissionCheckInterceptor)} for {nameof(PermissionCheckInterceptorTestClass)}.{nameof(PermissionCheckInterceptorTestClass.RunWithWorkspaceRelativityCase)} InnerExceptionType: Relativity.Core.Exception.Permission, InnerExceptionMessage: Logged in user ID 10240454 does not have access to workspace ID 1018995.  If the logged in user is a member of the system administrators group, please ensure that the logged in user is added to any other group with access to this workspace.");
		}


		[Test]
		public void ShouldThrowNotFoundExceptionWhenHasAdminOperationPermissionReturnsExceptionRelatedToWorskpaceUpgrating()
		{
			var workspaceId = Any.Integer();
			var exception = Any.InstanceOf<WorkspaceStatusException>();
			var serviceContext = Any.InstanceOf<BaseServiceContext>();
			var expectedErrorMessage =
				$"Error during call PermissionCheckInterceptor.EnsureUserHasPermissionsToUseWebApiReplacement. InnerExceptionType: Relativity.Core.Exception.WorkspaceStatusException, InnerExceptionMessage: {exception.Message}";
			_serviceContextFactoryMock.Setup(x => x.GetBaseServiceContext(workspaceId)).Returns(serviceContext);

			_relativityPermissionHelper
				.Setup(x => x.HasAdminOperationPermission(serviceContext, It.IsAny<Permission>()))
				.Throws(exception);

			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceCamelCase(workspaceId)).Should()
				.Throw<NotFoundException>()
				.WithMessage(expectedErrorMessage)
				.WithInnerException<WorkspaceStatusException>();
			
			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceLowerCase(workspaceId)).Should()
				.Throw<NotFoundException>()
				.WithMessage(expectedErrorMessage)
				.WithInnerException<WorkspaceStatusException>();

			FluentActions.Invoking(() => _interceptedObject.RunWithWorkspaceRelativityCase(workspaceId)).Should()
				.Throw<NotFoundException>()
				.WithMessage(expectedErrorMessage)
				.WithInnerException<WorkspaceStatusException>();
		}
	}
}
