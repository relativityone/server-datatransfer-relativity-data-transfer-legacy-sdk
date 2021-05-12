using Atata;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Web;
using Relativity.Testing.Framework.Web.Components;
using Relativity.Testing.Identification;
using System;

namespace RAPTemplate.FunctionalTests.CI
{
	[TestFixture]
	[TestExecutionCategory.CI]
	public class WebTests
	{
		[OneTimeSetUp]
		public void SetUp()
		{
			RelativityFacade.Instance.RelyOn<WebComponent>();

			string chromeDriverPath = RelativityFacade.Instance.Config.GetValue<string>("ChromeBinaryLocation");
			AtataContextBuilder contextBuilder = AtataContext.Configure().UseChrome().WithDriverPath(chromeDriverPath).WithCommandTimeout(TimeSpan.FromSeconds(180));
			contextBuilder.Build();
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			AtataContext.Current?.Dispose();
		}

		[Test]
		public void RAPTemplateWebTest()
		{
			string userEmail = RelativityFacade.Instance.Config.RelativityInstance.AdminUsername;
			string userPassword = RelativityFacade.Instance.Config.RelativityInstance.AdminPassword;		

			Go.To<LoginPage>()
				.EnterCredentials(userEmail, userPassword)
				.Login.ClickAndGo(new OrdinaryPage("Admin Landing"))
				.Content.Should.Contain("Workspaces");
		}
	}
}
