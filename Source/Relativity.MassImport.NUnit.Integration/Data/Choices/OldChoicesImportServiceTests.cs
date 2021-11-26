using System.Threading.Tasks;
using kCura.Data.RowDataGateway;
using NUnit.Framework;
using Relativity;
using Relativity.MassImport;
using Relativity.MassImport.Data.Choices;

namespace MassImport.NUnit.Integration.Data.Choices
{
	[TestFixture]
	public class OldChoicesImportServiceTests : ChoicesImportServiceTestBase
	{
		private protected override IChoicesImportService CreateSut(NativeLoadInfo settings)
		{
			return new OldChoicesImportService(
				EddsdboContext,
				ToggleProviderMock.Object,
				TableNames,
				ImportMeasurements,
				settings,
				QueryTimeoutInSeconds);
		}

		protected override Task SetOverlayBehaviorForFieldAsync(FieldInfo choiceField, OverlayBehavior overlayBehavior)
		{
			string overlayMergeValues = overlayBehavior == OverlayBehavior.MergeAll ? "1" : "0";
			var query = new QueryInformation
			{
				Statement = $@"IF EXISTS(SELECT 1 FROM [EDDSDBO].[Field] WHERE [ArtifactID] = {choiceField.ArtifactID})
BEGIN
	UPDATE [EDDSDBO].[Field] SET [OverlayMergeValues] = {overlayMergeValues} WHERE [ArtifactID] = {choiceField.ArtifactID};
END
ELSE BEGIN
	INSERT [EDDSDBO].[Field]([ArtifactID], [OverlayMergeValues]) VALUES ({choiceField.ArtifactID}, {overlayMergeValues});
END"
			};
			return EddsdboContext.ExecuteNonQueryAsync(query);
		}
	}
}
