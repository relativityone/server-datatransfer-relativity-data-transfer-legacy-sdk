//using NUnit.Framework;
//using RAPTemplate.Services;
//using Relativity.Testing.Framework;
//using Relativity.Testing.Framework.Api;
//using Relativity.Testing.Framework.Api.Kepler;
//using Relativity.Testing.Framework.RingSetup;
//using Relativity.Testing.Identification;

//namespace RAPTemplate.FunctionalTests.CD
//{
//	[IdentifiedTestFixture("92CB4376-51A2-46C1-8E1F-C635CF444321", Author = "The Authors", Description = "RapTemplate Functional Verification Tests.")]
//	public class APITests : TestSetup
//	{
//		public APITests() : base("RapTemplate", 10)
//		{ }

//		[IdentifiedTest("cb338a07-6d68-44b5-a70c-abcd99991234")]
//		[TestExecutionCategory.RAPCD.Verification.Functional]
//		public void RAPTemplateApiTest()
//		{
//			IKeplerServiceFactory serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;

//			bool result = false;
//			using (IRAPTemplateService RAPTemplateService = serviceFactory.GetServiceProxy<IRAPTemplateService>())
//			{
//				result = RAPTemplateService.IsAlive().GetAwaiter().GetResult();
//			}
//			Assert.IsTrue(result, "RAPTemplate's services are all running!.");
//		}
//	}
//}
