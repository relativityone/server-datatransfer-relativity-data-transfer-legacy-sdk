using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Identification;
using Any = TddEbook.TddToolkit.Any;
using IUserService = Relativity.Testing.Framework.Api.Services.IUserService;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CI
{
	[TestFixture]
	[TestExecutionCategory.CI]
	[TestType.Error]
	[TestLevel.L3]
	public class PermissionTests
	{
		private static Workspace _workspace;
		private static User _user;
		private IUserService _userService;
		private IGroupService _groupService;
		private IWorkspaceService _workspaceService;
		private Group _group;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			RelativityFacade.Instance.RelyOn<CoreComponent>();
			RelativityFacade.Instance.RelyOn<ApiComponent>();

			_userService = RelativityFacade.Instance.Resolve<IUserService>();
			_groupService = RelativityFacade.Instance.Resolve<IGroupService>();
			var workspacePermissionService = RelativityFacade.Instance.Resolve<IWorkspacePermissionService>();
			_workspaceService = RelativityFacade.Instance.Resolve<IWorkspaceService>();

			SetUpNoPermissionUser(workspacePermissionService);
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			_userService.Delete(_user.ArtifactID);
			_groupService.Delete(_group.ArtifactID);
			_workspaceService.Delete(_workspace.ArtifactID);
		}

		private void SetUpNoPermissionUser(IWorkspacePermissionService permissionService)
		{
			_group = _groupService.Create(new Group{ Name = $"NoPermissionGroup_{Any.Guid()}"});
			_user = _userService.Create(new User());
			var templateWorkspace = _workspaceService.Get("Relativity Starter Template");
			_workspace = _workspaceService.Create(new Workspace
				{Name = $"NoPermissionWorkspace_{Any.String()}", TemplateWorkspace = templateWorkspace});
			_userService.AddToGroup(_user.ArtifactID, _group.ArtifactID);
			permissionService.AddWorkspaceToGroup(_workspace.ArtifactID, _group.ArtifactID);
			permissionService.SetWorkspaceGroupPermissions(_workspace.ArtifactID,
				_group.ArtifactID, changeset => changeset.AdminPermissions.DisableAll());
		}

		[IdentifiedTest("5ADCC868-8E78-459E-9779-A34EBC695809")]
		[Description("Should throw permission denied exception when user has no permissions to import/export")]
		public async Task ShouldThrowPermissionDeniedExceptionWhenUserHasNoPermissionsToImportExport()
		{
			var keplerServiceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;
			using (var caseService = keplerServiceFactory.GetServiceProxy<ICaseService>(_user.EmailAddress, _user.Password))
			{
				await caseService.ReadAsync(_workspace.ArtifactID, Any.String());
			}
		}
	}
}
