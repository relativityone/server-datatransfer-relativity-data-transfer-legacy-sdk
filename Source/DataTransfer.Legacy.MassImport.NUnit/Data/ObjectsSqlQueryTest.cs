using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DataTransfer.Legacy.MassImport.NUnit.Properties;
using kCura.Data;
using Moq;
using NUnit.Framework;
using Relativity.Core;
using Relativity.MassImport.Core;
using Relativity.MassImport.Data;

namespace Relativity.MassImport.NUnit.Data
{
	using Relativity.API;

	[TestFixture]
	internal class ObjectsSqlQueryTest : BaseSqlQueryTest
	{
		private const string ObjectTableName = "ObjectTableName";

		private Objects _objects;

		[SetUp]
		public void SetUp()
		{
			Mock<ILockHelper> lockHelperMock = new Mock<ILockHelper>();
			lockHelperMock.Setup(m => m.Lock(It.IsAny<BaseContext>(), It.IsAny<MassImportManagerLockKey.LockType>(), It.IsAny<Action>()))
				.Callback<BaseContext, MassImportManagerLockKey.LockType, Action>((context, lockType, lockedAction) => { lockedAction.Invoke(); });

			_objects = new Objects(
				context: this.ContextMock.Object,
				queryExecutor: null,
				settings: InitializeSettings(),
				importUpdateAuditAction: 1, 
				importMeasurements: new ImportMeasurements(), 
				columnDefinitionCache: ColumnDefinitionCache, 
				caseSystemArtifactId: CaseArtifactId,
				new Mock<IHelper>().Object);
		}

		[Test]
		public void ShouldReturnCorrectCreateObjectsSqlStatement()
		{
			// arrange
			var readerMock = new Mock<IDataReader>();
			readerMock.Setup(x => x.Read()).Returns(true);
			readerMock.Setup(x => x["ArtifactType"]).Returns(ObjectTableName);

			const int singleObjectArtifactTypeId = 1000051;
			ContextMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>(), It.IsAny<int>())).Returns(CreateDataTableMock(singleObjectArtifactTypeId));
			ContextMock.Setup(x => x.ExecuteSqlStatementAsTReader<IDataReader>(It.IsAny<string>(), It.IsAny<ParameterList>(), It.IsAny<int>())).Returns(readerMock.Object);
			ColumnDefinitionCache.LoadDataFromCache(RunId);

			// act
			string actual = _objects.GetCreateObjectsSqlStatement(RequestOrigination, RecordOrigination, PerformAudit).ToString();

			// assert
			ThenSQLsAreEqual(actual, Resources.ObjectsSqlQueryTest_ExpectedCreateObjectsSqlStatement);
		}

		[Test]
		public void ShouldExecuteCorrectQueryWhenCreateAssociatedObjectsForSingleObjectFieldByName()
		{
			// arrange
			const int singleObjectArtifactTypeId = 1000051;
			ContextMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>(), It.IsAny<int>())).Returns(CreateDataTableMock(singleObjectArtifactTypeId));
			ContextMock.Setup(x => x.ExecuteSqlStatementAsScalar<string>(It.IsAny<string>(), It.IsAny<int>())).Returns(SingleObjectIdFieldColumnName);
			ColumnDefinitionCache.LoadDataFromCache(RunId);

			// act
			_objects.CreateAssociatedObjectsForSingleObjectFieldByName(SingleObjectField, UserId, AuditUserId, RequestOrigination, RecordOrigination, PerformAudit);
			
			// assert
			ContextMock.Verify(x => x.ExecuteNonQuerySQLStatement(
				It.Is<string>(sql => ThenSQLsAreEqual(sql, Resources.ObjectsSqlQueryTest_ExpectedCreateAssociatedObjectsForSingleObjectFieldByName)),
				It.IsAny<IEnumerable<SqlParameter>>(),
				It.IsAny<int>()));
		}

		[Test]
		public void ShouldExecuteCorrectQueryWhenCreateAssociatedObjectsForSelfReferencedSingleObjectFieldByName()
		{
			// arrange
			ContextMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>(), It.IsAny<int>())).Returns(CreateDataTableMock(ArtifactTypeId));
			ContextMock.Setup(x => x.ExecuteSqlStatementAsScalar<string>(It.IsAny<string>(), It.IsAny<int>())).Returns(SingleObjectIdFieldColumnName);
			ColumnDefinitionCache.LoadDataFromCache(RunId);

			// act
			_objects.CreateAssociatedObjectsForSingleObjectFieldByName(SingleObjectField, UserId, AuditUserId, RequestOrigination, RecordOrigination, PerformAudit);
			
			// assert
			ContextMock.Verify(x => x.ExecuteNonQuerySQLStatement(
				It.Is<string>(sql => ThenSQLsAreEqual(sql, Resources.ObjectsSqlQueryTest_ExpectedCreateAssociatedObjectsForSelfReferencedSingleObjectFieldByName)),
				It.IsAny<IEnumerable<SqlParameter>>(),
				It.IsAny<int>()));
		}

		[Test]
		public void ShouldExecuteCorrectQueryWhenCreateAssociatedObjectsForSingleObjectDocumentFieldByName()
		{
			// arrange
			const ArtifactType singleObjectArtifactTypeId = ArtifactType.Document;
			ContextMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>(), It.IsAny<int>())).Returns(CreateDataTableMock((int) singleObjectArtifactTypeId));
			ContextMock.Setup(x => x.ExecuteSqlStatementAsDataTable(It.IsAny<string>(), It.IsAny<ParameterList>())).Returns(CreateSimpleDataTableMock());
			ContextMock.Setup(x => x.ExecuteSqlStatementAsScalar<string>(It.IsAny<string>(), It.IsAny<int>())).Returns(SingleObjectIdFieldColumnName);
			ColumnDefinitionCache.LoadDataFromCache(RunId);

			// act
			_objects.CreateAssociatedObjectsForSingleObjectFieldByName(SingleObjectField, UserId, AuditUserId, RequestOrigination, RecordOrigination, PerformAudit);

			// assert
			ContextMock.Verify(x => x.ExecuteNonQuerySQLStatement(
				It.Is<string>(sql => ThenSQLsAreEqual(sql, Resources.ObjectsSqlQueryTest_ExpectedCreateAssociatedObjectsForSingleObjectDocumentFieldByName)),
				It.IsAny<IEnumerable<SqlParameter>>(),
				It.IsAny<int>()));
		}
	}
}