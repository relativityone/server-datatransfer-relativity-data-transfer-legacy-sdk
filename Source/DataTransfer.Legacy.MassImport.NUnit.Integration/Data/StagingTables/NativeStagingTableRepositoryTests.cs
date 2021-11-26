using NUnit.Framework;
using Relativity.MassImport.Data.StagingTables;

namespace MassImport.NUnit.Integration.Data.StagingTables
{
	[TestFixture]
	public class NativeStagingTableRepositoryTests : BaseStagingTableRepositoryTests
	{
		private protected override BaseStagingTableRepository CreateSut()
		{
			return new NativeStagingTableRepository(EddsdboContext, TableNames, ImportMeasurements);
		}
	}
}
