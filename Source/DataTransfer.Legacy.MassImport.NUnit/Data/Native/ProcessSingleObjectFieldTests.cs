using NUnit.Framework;

namespace Relativity.MassImport.NUnit.Data.Native
{
	public class ProcessSingleObjectFieldTests
	{
		private MockOfNative _mock;

		[SetUp()]
		public void SetUp()
		{
			_mock = new MockOfNative();
		}

		[Test()]
		public void ObjectFieldContainsArtifactId_VerifyExistenceOfAssociatedObjectsForSingleObjectFieldByArtifactIdIsCalled()
		{
			var field = new FieldInfo();
			field.ImportBehavior = FieldInfo.ImportBehaviorChoice.ObjectFieldContainsArtifactId;

			_mock.ProcessSingleObjectField(field, 0, 0, "", "", false, 0);

			Assert.IsTrue(_mock._verifyExistenceOfAssociatedObjectsForSingleObjectFieldByArtifactIdWasCalled);
			Assert.IsFalse(_mock._createAssociatedObjectsForSingleObjectFieldByNameWasCalled);
		}

		[Test()]
		public void ObjectFieldDoesNotContainsArtifactId_CreateAssociatedObjectsForSingleObjectFieldByNameIsCalled()
		{
			var field = new FieldInfo();

			_mock.ProcessSingleObjectField(field, 0, 0, "", "", false, 0);

			Assert.IsTrue(_mock._createAssociatedObjectsForSingleObjectFieldByNameWasCalled);
			Assert.IsFalse(_mock._verifyExistenceOfAssociatedObjectsForSingleObjectFieldByArtifactIdWasCalled);
		}
	}

	public class MockOfNative : Relativity.Data.MassImportOld.Native
	{
		internal bool _verifyExistenceOfAssociatedObjectsForSingleObjectFieldByArtifactIdWasCalled = false;
		internal bool _createAssociatedObjectsForSingleObjectFieldByNameWasCalled = false;
		internal bool _verifyExistenceOfAssociatedObjectsForMultiObjectFieldByArtifactIdWasCalled = false;
		internal bool _createAssociatedObjectsForMultiObjectFieldByNameWasCalled = false;

		public MockOfNative() : base(null, new NativeLoadInfo())
		{
		}

		public override void VerifyExistenceOfAssociatedObjectsForSingleObjectFieldByArtifactId(FieldInfo field, int userID, int? auditUserId)
		{
			_verifyExistenceOfAssociatedObjectsForSingleObjectFieldByArtifactIdWasCalled = true;
		}

		public override void CreateAssociatedObjectsForSingleObjectFieldByName(FieldInfo field, int userID, int? auditUserId, string requestOrigination, string recordOrigination, bool performAudit, int caseSystemArtifactID)
		{
			_createAssociatedObjectsForSingleObjectFieldByNameWasCalled = true;
		}

		public override void VerifyExistenceOfAssociatedObjectsForMultiObjectByArtifactId(FieldInfo field, int userID, int? auditUserId)
		{
			_verifyExistenceOfAssociatedObjectsForMultiObjectFieldByArtifactIdWasCalled = true;
		}

		public override void CreateAssociatedObjectsForMultiObjectFieldByName(FieldInfo field, int userID, string requestOrigination, string recordOrigination, bool performAudit)
		{
			_createAssociatedObjectsForMultiObjectFieldByNameWasCalled = true;
		}
	}
}