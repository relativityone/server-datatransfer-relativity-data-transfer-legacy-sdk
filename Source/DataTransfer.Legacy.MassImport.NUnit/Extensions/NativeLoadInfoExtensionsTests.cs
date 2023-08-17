using System;
using NUnit.Framework;
using Relativity.MassImport.Extensions;

namespace Relativity.MassImport.NUnit.Extensions
{
	[TestFixture]
	public class NativeLoadInfoExtensionsTests
	{
		[Test]
		public void GetKeyField_ShouldReturnKeyField()
		{
			// arrange
			int keyFieldArtifactId = 7;
			var keyField = new FieldInfo { ArtifactID = keyFieldArtifactId };
			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				MappedFields = new[]
				{
					new FieldInfo{ArtifactID = 1},
					keyField,
					new FieldInfo{ArtifactID = 2}
				},
				KeyFieldArtifactID = keyFieldArtifactId
			};

			// act
			var actual = settings.GetKeyField();

			// assert
			Assert.That(actual, Is.EqualTo(keyField));
		}

		[Test]
		public void GetKeyField_ShouldReturnFirstKeyField()
		{
			// arrange
			int keyFieldArtifactId = 7;
			var firstkeyField = new FieldInfo { ArtifactID = keyFieldArtifactId };
			var secondkeyField = new FieldInfo { ArtifactID = keyFieldArtifactId };
			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				MappedFields = new[]
				{
					new FieldInfo{ArtifactID = 1},
					firstkeyField,
					secondkeyField,
					new FieldInfo{ArtifactID = 2}
				},
				KeyFieldArtifactID = keyFieldArtifactId
			};

			// act
			var actual = settings.GetKeyField();

			// assert
			Assert.That(actual, Is.EqualTo(firstkeyField));
		}

		[Test]
		public void GetKeyField_ShouldReturnNullWhenKeyIsNotMapped()
		{
			// arrange
			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				MappedFields = new[]
				{
					new FieldInfo{ArtifactID = 7}
				},
				KeyFieldArtifactID = 8
			};

			// act
			var actual = settings.GetKeyField();

			// assert
			Assert.That(actual, Is.Null);
		}

		[Test]
		public void GetKeyField_ShouldReturnNullWhenMappedFieldsArrayIsEmpty()
		{
			// arrange
			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				MappedFields = new FieldInfo[0]
			};

			// act
			var actual = settings.GetKeyField();

			// assert
			Assert.That(actual, Is.Null);
		}

		[Test]
		public void GetKeyField_ShouldReturnNullWhenMappedFieldsArrayIsNull()
		{
			// arrange
			var settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				MappedFields = null
			};

			// act
			var actual = settings.GetKeyField();

			// assert
			Assert.That(actual, Is.Null);
		}

		[Test]
		public void GetKeyField_ShouldThrowExceptionForNullInstance()
		{
			// arrange
			Relativity.MassImport.DTO.NativeLoadInfo settings = null;

			// act&assert
			Assert.Throws<ArgumentNullException>(() => settings.GetKeyField());
		}
	}
}
