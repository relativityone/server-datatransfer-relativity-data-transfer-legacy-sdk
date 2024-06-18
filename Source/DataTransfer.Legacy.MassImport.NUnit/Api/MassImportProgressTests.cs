using NUnit.Framework;
using Relativity.MassImport.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer.Legacy.MassImport.NUnit.Api
{
	[TestFixture]
	public class MassImportProgressTests
	{
		private List<int> _affectedArtifactIds;

		[SetUp]
		public void SetUp()
		{
			_affectedArtifactIds = new List<int> { 1, 2, 3, 4, 5 };
		}

		[Test]
		public void Constructor_ShouldInitializeProperties()
		{
			// Act
			var progress = new MassImportProgress(_affectedArtifactIds);

			// Assert
			Assert.AreEqual(_affectedArtifactIds, progress.AffectedArtifactIds);
		}

		[Test]
		public void Constructor_ShouldHandleEmptyList()
		{
			// Arrange
			var emptyList = new List<int>();

			// Act
			var progress = new MassImportProgress(emptyList);

			// Assert
			Assert.AreEqual(emptyList, progress.AffectedArtifactIds);
			Assert.IsEmpty(progress.AffectedArtifactIds);
		}

		[Test]
		public void Constructor_ShouldHandleNull()
		{
			// Act
			var progress = new MassImportProgress(null);

			// Assert
			Assert.IsNull(progress.AffectedArtifactIds);
		}
	}
}