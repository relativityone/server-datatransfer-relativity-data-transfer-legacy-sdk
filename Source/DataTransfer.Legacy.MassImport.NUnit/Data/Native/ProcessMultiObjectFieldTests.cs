using NUnit.Framework;

namespace Relativity.MassImport.NUnit.Data.Native
{
	public class ProcessMultiObjectFieldTests
	{
		private MockOfNative _mock;

		[SetUp()]
		public void SetUp()
		{
			_mock = new MockOfNative();
		}

		[Test()]
		public void ObjectFieldContainsArtifactId_VerifyExistenceOfAssociatedObjectsForMultiObjectFieldByArtifactIdIsCalled()
		{
			var field = new FieldInfo();
			field.ImportBehavior = FieldInfo.ImportBehaviorChoice.ObjectFieldContainsArtifactId;

			_mock.ProcessMultiObjectField(field, 0, 0, "", "", false, 0);

			Assert.IsTrue(_mock._verifyExistenceOfAssociatedObjectsForMultiObjectFieldByArtifactIdWasCalled);
			Assert.IsFalse(_mock._createAssociatedObjectsForMultiObjectFieldByNameWasCalled);
		}

		[Test()]
		public void ObjectFieldDoesNotContainsArtifactId_CreateAssociatedObjectsForMultiObjectFieldByNameIsCalled()
		{
			var field = new FieldInfo();

			_mock.ProcessMultiObjectField(field, 0, 0, "", "", false, 0);

			Assert.IsTrue(_mock._createAssociatedObjectsForMultiObjectFieldByNameWasCalled);
			Assert.IsFalse(_mock._verifyExistenceOfAssociatedObjectsForMultiObjectFieldByArtifactIdWasCalled);
		}
	}
}