using NUnit.Framework;
using Relativity.MassImport.Core.Pipeline.Framework.Stages;
using Relativity.MassImport.Core.Pipeline.Input;
using Relativity.MassImport.Core.Pipeline.Stages.Shared;

namespace Relativity.MassImport.NUnit.Core.Pipeline.Framework.Stages
{
	[TestFixture]
	public class IPipelineStageExtensionsTests
	{
		[Test]
		public void GetUserFriendlyStageNameShouldReturnCorrectNameForNonGenericType()
		{
			// arrange
			const string expectedName = "CopyExtratedTextFilesToDataGridStage";
			var sut = new CopyExtratedTextFilesToDataGridStage(context: null);

			// act
			string actualName = sut.GetUserFriendlyStageName();

			// assert
			Assert.That(actualName, Is.EqualTo(expectedName));
		}

		[Test]
		public void GetUserFriendlyStageNameShouldReturnCorrectNameForTypeWithOneGenericParameter()
		{
			// arrange
			const string expectedName = "ValidateSettingsStage";
			var sut = new ValidateSettingsStage<NativeImportInput>();

			// act
			string actualName = sut.GetUserFriendlyStageName();

			// assert
			Assert.That(actualName, Is.EqualTo(expectedName));
		}

		[Test]
		public void GetUserFriendlyStageNameShouldReturnCorrectNameForTypeWithTwoGenericParameters()
		{
			// arrange
			const string expectedName = "ExecuteInTransactionDecoratorStage";
			var sut = new ExecuteInTransactionDecoratorStage<int, string>(pipelineExecutor: null, innerStage: null, context: null);

			// act
			string actualName = sut.GetUserFriendlyStageName();

			// assert
			Assert.That(actualName, Is.EqualTo(expectedName));
		}
	}
}
