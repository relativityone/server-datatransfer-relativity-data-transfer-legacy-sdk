using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Relativity.Core;
using Relativity.Core.Service.ObjectRule;
using Relativity.MassImport.Core;
using Relativity.MassImport.Data;
using System.Xml.Linq;
using DataTransfer.Legacy.MassImport.NUnit.Properties;

namespace Relativity.MassImport.NUnit.Data
{
	[TestFixture]
	internal class NativeSqlQueryTest : BaseSqlQueryTest
	{
		private MassImport.Data.Native _native;
		
		[SetUp]
		public void SetUp()
		{
			Mock<BaseContext> baseContextMock = new Mock<BaseContext>();
			baseContextMock.Setup(m => m.DBContext).Returns(ContextMock.Object);
			Mock<ILockHelper> lockHelperMock = new Mock<ILockHelper>();
			lockHelperMock.Setup(m => m.Lock(It.IsAny<BaseContext>(),It.IsAny<MassImportManagerLockKey.LockType>(), It.IsAny<Action>()))
				.Callback<BaseContext, MassImportManagerLockKey.LockType, Action>((context, lockType, lockedAction) => { lockedAction.Invoke(); });
			
			_native = new MassImport.Data.Native(
				context: baseContextMock.Object, 
				queryExecutor: null,
				settings: InitializeSettings(), 
				importUpdateAuditAction: 1, 
				importMeasurements: new ImportMeasurements(),
				columnDefinitionCache: ColumnDefinitionCache,
				caseSystemArtifactId: CaseArtifactId,
				lockHelper: lockHelperMock.Object);

			_native.AllRelationalColumns = new FieldInfo[] { };
		}

		[Test]
		public void ShouldReturnCorrectCreateDocumentsSqlStatement()
		{
			// arrange
			const bool includeExtractedTextEncoding = false;
			const string codeArtifactTableName = "CodeArtifactTableName";

			// act
			string actual = _native.GetCreateDocumentsSqlStatement(RequestOrigination, RecordOrigination, PerformAudit, includeExtractedTextEncoding, codeArtifactTableName).ToString();
			
			// assert
			ThenSQLsAreEqual(actual, Resources.NativeSqlQueryTest_ExpectedCreateDocumentsSqlStatement);
		}

		[Test]
		public void ShouldReturnCorrectCreateDocumentsSqlStatementWithExtractedText()
		{
			// arrange
			const bool includeExtractedTextEncoding = true;
			const string codeArtifactTableName = "CodeArtifactTableName";

			// act
			string actual = _native.GetCreateDocumentsSqlStatement(RequestOrigination, RecordOrigination, PerformAudit, includeExtractedTextEncoding, codeArtifactTableName).ToString();
			
			// assert
			ThenSQLsAreEqual(actual, Resources.NativeSqlQueryTest_ExpectedCreateDocumentsSqlStatementWithExtractedText);
		}

		[Test] 
		[Ignore("Local build successful, but Jenkins build fails")]
		public void ShouldExecuteCorrectQueryWhenCreateAssociatedObjectsForSingleObjectFieldByName()
		{
			// arrange
			const int singleObjectArtifactTypeId = 1000051;
			ContextMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>(), It.IsAny<int>())).Returns(CreateDataTableMock(singleObjectArtifactTypeId));
			ColumnDefinitionCache.LoadDataFromCache(RunId);

			// act
			_native.CreateAssociatedObjectsForSingleObjectFieldByName(SingleObjectField, UserId, AuditUserId, RequestOrigination, RecordOrigination, PerformAudit);

			// assert
			ContextMock.Verify(context => context.ExecuteNonQuerySQLStatement(
				It.Is<string>(sql => ThenSQLsAreEqual(sql, Resources.NativeSqlQueryTest_ExpecteCreateAssociatedObjectsForSingleObjectFieldByName)),
				It.IsAny<IEnumerable<SqlParameter>>(),
				It.IsAny<int>()));
		}
	}
}