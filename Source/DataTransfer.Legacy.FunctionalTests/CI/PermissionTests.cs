using System.Runtime.InteropServices;
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

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			var userService = RelativityFacade.Instance.Resolve<IUserService>();
			var groupService = RelativityFacade.Instance.Resolve<IGroupService>();
			var permissionService = RelativityFacade.Instance.Resolve<IPermissionService>();
			var workspaceService = RelativityFacade.Instance.Resolve<IWorkspaceService>();

			SetUpNoPermissionUser(workspaceService, groupService, userService, permissionService);
		}

		private static void SetUpNoPermissionUser(IWorkspaceService workspaceService, IGroupService groupService,
			IUserService userService, IPermissionService permissionService)
		{
			_workspace = workspaceService.Create(new Workspace());
			var group = groupService.Create(new Group());
			_user = userService.Create(new User());
			userService.AddToGroup(_user.ArtifactID, group.ArtifactID);
			permissionService.WorkspacePermissionService.AddWorkspaceToGroup(_workspace.ArtifactID, group.ArtifactID);
			permissionService.WorkspacePermissionService.SetWorkspaceGroupPermissions(_workspace.ArtifactID,
				group.ArtifactID, changeset => changeset.AdminPermissions.DisableAll());
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
