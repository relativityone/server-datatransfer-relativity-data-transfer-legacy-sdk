using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Core;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Tests.Interceptors.TestClasses;
using Relativity.Services.Objects.Exceptions;
using TddEbook.TddToolkit;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Interceptors
{
	[TestFixture]
	public class PermissionCheckInterceptorTests
	{
		private Mock<IServiceContextFactory> _serviceContextFactoryMock;
		private IPermissionCheckInterceptorTestClass _interceptedObject;
		private Mock<IRelativityPermissionHelper> _relativityPermissionHelper;

		[SetUp]
		public void SetUp()
		{
			_serviceContextFactoryMock = new Mock<IServiceContextFactory>();
			_relativityPermissionHelper = new Mock<IRelativityPermissionHelper>();

			var container = new WindsorContainer();
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
		public void ShouldNotCheckPermissionsWhenRunMethodWithWorkspaceIdArgumentSetToAnyIntAndArgCasingIsUnrecognized()
		{
			_interceptedObject.RunWithWorkspaceUnrecognizedCase(Any.Integer());

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
	}
}
