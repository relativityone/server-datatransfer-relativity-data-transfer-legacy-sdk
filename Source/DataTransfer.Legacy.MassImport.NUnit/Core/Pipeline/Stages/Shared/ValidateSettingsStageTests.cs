using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using NUnit.Framework;
using Relativity.MassImport.Core.Pipeline.Input.Interface;
using Relativity.MassImport.Core.Pipeline.Stages.Shared;

namespace Relativity.MassImport.NUnit.Core.Pipeline.Stages.Shared
{
	[TestFixture]
	public class ValidateSettingsStageTests
	{
		private readonly Mock<IImportSettingsInput<Relativity.MassImport.DTO.NativeLoadInfo>> _inputMock;
		private readonly ValidateSettingsStage<IImportSettingsInput<Relativity.MassImport.DTO.NativeLoadInfo>> _sut;

		private Relativity.MassImport.DTO.NativeLoadInfo _settings;

		public ValidateSettingsStageTests()
		{
			_sut = new ValidateSettingsStage<IImportSettingsInput<Relativity.MassImport.DTO.NativeLoadInfo>>();

			_inputMock = new Mock<IImportSettingsInput<Relativity.MassImport.DTO.NativeLoadInfo>>();
			_inputMock
				.Setup(x => x.Settings)
				.Returns(() => _settings);
		}

		[SetUp]
		public void SetUp()
		{
			string validRunId = ValidRunIds().First();
			string validFileName = ValidFileNames().First();

			_settings = new Relativity.MassImport.DTO.NativeLoadInfo
			{
				RunID = validRunId,
				CodeFileName = validFileName,
				DataFileName = validFileName,
				ObjectFileName = validFileName,
				DataGridFileName = validFileName
			};
		}

		[TestCaseSource(nameof(ValidRunIds))]
		public void ShouldNotThrowWhenRunIdIsValid(string runId)
		{
			// arrange
			_settings.RunID = runId;

			// act
			void ValidateAction() => _sut.Execute(_inputMock.Object);

			// assert
			Assert.DoesNotThrow(ValidateAction, "Input was valid");
		}

		[TestCaseSource(nameof(InvalidRunIds))]
		public void ShouldThrowWhenRunIdIsInvalid(string runId)
		{
			// arrange
			_settings.RunID = runId;

			// act
			void ValidateAction() => _sut.Execute(_inputMock.Object);

			// assert
			const string expectedErrorMessage = "Invalid RunId";
			System.Exception actualException = Assert.Throws<System.Exception>(ValidateAction);
			Assert.That(
				actualException.Message,
				Is.EqualTo(expectedErrorMessage),
				"Exception message was incorrect");
		}

		[TestCaseSource(nameof(ValidFileNames))]
		public void ShouldNotThrowWhenCodeFileNameIsValid(string fileName)
		{
			// arrange
			_settings.CodeFileName = fileName;

			// act
			void ValidateAction() => _sut.Execute(_inputMock.Object);

			// assert
			Assert.DoesNotThrow(ValidateAction, "Input was valid");
		}

		[TestCaseSource(nameof(InvalidFileNames))]
		public void ShouldThrowWhenCodeFileNameIsInvalid(string fileName)
		{
			// arrange
			_settings.CodeFileName = fileName;

			// act
			void ValidateAction() => _sut.Execute(_inputMock.Object);

			// assert
			const string expectedErrorMessage = "Invalid CodeFileName";
			System.Exception actualException = Assert.Throws<System.Exception>(ValidateAction);
			Assert.That(
				actualException.Message,
				Is.EqualTo(expectedErrorMessage),
				"Exception message was incorrect");
		}

		[TestCaseSource(nameof(ValidFileNames))]
		public void ShouldNotThrowWhenDataFileNameIsValid(string fileName)
		{
			// arrange
			_settings.DataFileName = fileName;

			// act
			void ValidateAction() => _sut.Execute(_inputMock.Object);

			// assert
			Assert.DoesNotThrow(ValidateAction, "Input was valid");
		}

		[TestCaseSource(nameof(InvalidFileNames))]
		public void ShouldThrowWhenDataFileNameIsInvalid(string fileName)
		{
			// arrange
			_settings.DataFileName = fileName;

			// act
			void ValidateAction() => _sut.Execute(_inputMock.Object);

			// assert
			const string expectedErrorMessage = "Invalid DataFileName";
			System.Exception actualException = Assert.Throws<System.Exception>(ValidateAction);
			Assert.That(
				actualException.Message,
				Is.EqualTo(expectedErrorMessage),
				"Exception message was incorrect");
		}

		[TestCaseSource(nameof(ValidFileNames))]
		public void ShouldNotThrowWhenObjectFileNameIsValid(string fileName)
		{
			// arrange
			_settings.ObjectFileName = fileName;

			// act
			void ValidateAction() => _sut.Execute(_inputMock.Object);

			// assert
			Assert.DoesNotThrow(ValidateAction, "Input was valid");
		}

		[TestCaseSource(nameof(InvalidFileNames))]
		public void ShouldThrowWhenObjectFileNameIsInvalid(string fileName)
		{
			// arrange
			_settings.ObjectFileName = fileName;

			// act
			void ValidateAction() => _sut.Execute(_inputMock.Object);

			// assert
			const string expectedErrorMessage = "Invalid ObjectFileName";
			System.Exception actualException = Assert.Throws<System.Exception>(ValidateAction);
			Assert.That(
				actualException.Message,
				Is.EqualTo(expectedErrorMessage),
				"Exception message was incorrect");
		}

		[TestCaseSource(nameof(ValidDataGridFileNames))]
		public void ShouldNotThrowWhenDataGridFileNameIsValid(string fileName)
		{
			// arrange
			_settings.DataGridFileName = fileName;
			_settings.LinkDataGridRecords = true;
			_settings.MappedFields = null;

			// act
			void ValidateAction() => _sut.Execute(_inputMock.Object);

			// assert
			Assert.DoesNotThrow(ValidateAction, "Input was valid");
		}

		[TestCaseSource(nameof(InvalidDataGridFileNames))]
		public void ShouldNotThrowWhenDataGridFileNameIsInvalidButDataGridIsNotUsed(string fileName)
		{
			// arrange
			_settings.DataGridFileName = fileName;
			_settings.LinkDataGridRecords = false;
			_settings.MappedFields = null;

			// act
			void ValidateAction() => _sut.Execute(_inputMock.Object);

			// assert
			Assert.DoesNotThrow(ValidateAction, "Input was valid");
		}

		[TestCaseSource(nameof(InvalidDataGridFileNames))]
		public void ShouldThrowWhenDataGridFileNameIsInvalidAndHasDataGridWorkToDo(string fileName)
		{
			// arrange
			_settings.DataGridFileName = fileName;
			_settings.LinkDataGridRecords = true;
			_settings.MappedFields = null;

			// act
			void ValidateAction() => _sut.Execute(_inputMock.Object);

			// assert
			const string expectedErrorMessage = "Invalid DataGridFileName";
			System.Exception actualException = Assert.Throws<System.Exception>(ValidateAction);
			Assert.That(
				actualException.Message,
				Is.EqualTo(expectedErrorMessage),
				"Exception message was incorrect");
		}

		private static IEnumerable<string> ValidRunIds()
		{
			yield return string.Empty;
			yield return Guid.NewGuid().ToString();
			yield return Guid.NewGuid().ToString().Replace("-", "_");
		}

		private static IEnumerable<string> InvalidRunIds()
		{
			yield return null;
			yield return "1";
			yield return "ID";
		}

		private static IEnumerable<string> InvalidFileNames()
		{
			yield return null;
			yield return "File/Name.txt";
			yield return @"File\Name.txt";
			yield return $"File{Path.GetInvalidFileNameChars().First()}Name.txt";
		}

		private static IEnumerable<string> ValidFileNames()
		{
			yield return string.Empty;
			yield return "FileName";
			yield return "FileName.txt";
			yield return Guid.NewGuid().ToString();
		}

		private static IEnumerable<string> InvalidDataGridFileNames()
		{
			// empty string is not a valid data grid file name.
			return InvalidFileNames().Concat(new[] { string.Empty });
		}

		private static IEnumerable<string> ValidDataGridFileNames()
		{
			// empty string is not a valid data grid file name.
			return ValidFileNames().Where(x => x != string.Empty);
		}
	}
}
