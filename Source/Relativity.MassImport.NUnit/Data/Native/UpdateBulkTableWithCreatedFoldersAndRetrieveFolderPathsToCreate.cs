using System.Data;
using Moq;
using NUnit.Framework;
using Relativity.MassImport.NUnit.Properties;

namespace Relativity.MassImport.NUnit.Data.Native
{
	[TestFixture]
	internal class UpdateBulkTableWithCreatedFoldersAndRetrieveFolderPathsToCreate : BaseSqlQueryTest
	{
		[Test]
		public void GeneratesCorrectStatement()
		{
			// arrange
			string actualStatement = null;
			ContextMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>(), It.IsAny<int>()))
				.Callback((string statement, int timeout) => actualStatement = statement)
				.Returns(new DataTable());

			var settings = new NativeLoadInfo();
			settings.RunID = "76a13eaa-430c-4c9a-9009-4fd046114fe6";
			var sut = new Relativity.Data.MassImportOld.Native(ContextMock.Object, settings) { QueryTimeout = 100 };

			// act & assert
			sut.ToggleProvider = new Relativity.Toggles.Providers.AlwaysDisabledToggleProvider();
			sut.UpdateBulkTableWithCreatedFoldersAndRetrieveFolderPathsToCreate(100, 100);
			ThenSQLsAreEqual(actualStatement, Resources.UpdateBulkTableWithCreatedFoldersAndRetrieveFolderPathsToCreate_ToggleOff);

			sut.ToggleProvider = new Relativity.Toggles.Providers.AlwaysEnabledToggleProvider();
			sut.UpdateBulkTableWithCreatedFoldersAndRetrieveFolderPathsToCreate(100, 100);
			ThenSQLsAreEqual(actualStatement, Resources.UpdateBulkTableWithCreatedFoldersAndRetrieveFolderPathsToCreate_ToggleOff);
		}
	}
}