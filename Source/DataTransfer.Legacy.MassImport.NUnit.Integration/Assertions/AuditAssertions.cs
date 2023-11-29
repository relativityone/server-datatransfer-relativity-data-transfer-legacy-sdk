using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using AuditAction = MassImport.NUnit.Integration.Helpers.AuditAction;
using AuditHelper = MassImport.NUnit.Integration.Helpers.AuditHelper;

namespace MassImport.NUnit.Integration.Assertions
{
	public class AuditAssertions
	{
		public static void ThenTheAuditIsCorrectAsync(TestWorkspace testWorkspace, int userId, List<Dictionary<string, string>> expectedAuditsDetails, int nrOfLastAuditsToTake, int largetThanThisAuditId, AuditAction action)
		{
			var auditsDetails = AuditHelper.GetAuditDetails(
				testWorkspace,
				action,
				largetThanThisAuditId,
				nrOfLastAuditsToTake,
				userId);

			var actualAuditDetails = auditsDetails.OrderBy(x => x["ArtifactID"]).ToList();

			Assert.AreEqual(expectedAuditsDetails.Count, actualAuditDetails.Count);

			for (int i = 0; i < actualAuditDetails.Count; i++)
			{
				var expectedSingleAudit = expectedAuditsDetails[i];
				var actualSingleAudit = actualAuditDetails[i];

				foreach (string key in expectedSingleAudit.Keys)
				{
					Assert.AreEqual(expectedSingleAudit[key], actualSingleAudit[key], GetAuditMessage(expectedAuditsDetails, actualAuditDetails));
				}
			}
		}

		private static string GetAuditMessage(
			List<Dictionary<string, string>> expectedAuditsDetails,
			List<Dictionary<string, string>> actualAuditDetails)
		{
			var builder = new StringBuilder();

			for (int i = 0; i < actualAuditDetails.Count; i++)
			{
				builder.AppendLine($"{i}");
				var expectedSingleAudit = expectedAuditsDetails[i];
				var actualSingleAudit = actualAuditDetails[i];

				foreach (var key in expectedSingleAudit.Keys)
				{
					builder.AppendLine($"{key}::expected:{expectedSingleAudit[key]};actual:{actualSingleAudit[key]}");
				}
			}

			return builder.ToString();
		}
	}
}
