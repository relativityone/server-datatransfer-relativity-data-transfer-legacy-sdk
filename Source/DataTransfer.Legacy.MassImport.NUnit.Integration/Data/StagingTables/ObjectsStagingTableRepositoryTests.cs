using NUnit.Framework;
using Relativity.MassImport.Data.StagingTables;

namespace MassImport.NUnit.Integration.Data.StagingTables
{
	[TestFixture]
	internal class ObjectsStagingTableRepositoryTests : BaseStagingTableRepositoryTests
	{
		private protected override BaseStagingTableRepository CreateSut()
		{
			return new ObjectsStagingTableRepository(EddsdboContext, TableNames, ImportMeasurements);
		}
	}
}
