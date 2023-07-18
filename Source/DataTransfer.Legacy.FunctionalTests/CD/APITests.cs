using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.RingSetup;
using Relativity.Testing.Identification;

namespace Relativity.DataTransfer.Legacy.FunctionalTests.CD
{
	[TestExecutionCategory.CD]
	[TestLevel.L3]
	[IdentifiedTestFixture("1068299A-A7E9-469E-BD28-0F2AFDD9CE65")]
	public class APITests : TestSetup
	{
		public APITests() 
			: base("DataTransfer.Legacy", desiredNumberOfDocuments: 0, importImages: false)
		{ }

		[IdentifiedTest("94CE4420-EF1F-47C2-87BC-907468D66CE3")]
		[TestExecutionCategory.RAPCD.Verification.Functional]
		public async Task DataTransferLegacyApiTest()
		{
			var serviceFactory = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory;

			using (var iapiCommunicationModeService = serviceFactory.GetServiceProxy<IIAPICommunicationModeService>())
			{
				IAPICommunicationMode result = await iapiCommunicationModeService.GetIAPICommunicationModeAsync(Guid.NewGuid().ToString());

				Assert.NotNull(result);
			}
		}
	}
}
