using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Relativity.Services.Interfaces.Choice;
using Relativity.Services.Interfaces.Choice.Models;
using Relativity.Services.Interfaces.Shared.Models;

namespace MassImport.NUnit.Integration.Helpers
{
	public static class ChoiceHelper
	{
		public static async Task<Dictionary<string, int>> CreateChoiceValuesAsync(
			IntegrationTestParameters testParameters,
			TestWorkspace testWorkspace,
			int choiceFieldId,
			IEnumerable<string> choiceValues)
		{
			var massCreateChoiceRequest = new MassCreateChoiceRequest
			{
				ChoiceTemplateData = new MassCreateChoiceModel
				{
					Field = new ObjectIdentifier { ArtifactID = choiceFieldId },
					Keywords = "",
					Notes = "",
				},
				Choices = choiceValues.Select(x => new MassCreateChoiceStructure { Name = x }).ToList()
			};

			MassCreateChoiceResponse response;
			using (var choiceManager = ServiceHelper.GetServiceProxy<IChoiceManager>(testParameters))
			{
				response = await choiceManager.CreateAsync(testWorkspace.WorkspaceId, massCreateChoiceRequest).ConfigureAwait(false);
			}

			if (!response.Success)
			{
				Assert.Fail($"Error occured while creating values for choices: {response.Message}");
			}

			return response.ChoicesCreated.ToDictionary(x => x.Name, x => x.ArtifactID);
		}
	}
}
