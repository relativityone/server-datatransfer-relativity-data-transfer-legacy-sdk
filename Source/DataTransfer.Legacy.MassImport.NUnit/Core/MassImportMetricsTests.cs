﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework;
using Relativity.Logging;
using Relativity.MassImport.Core;
using Relativity.Telemetry.APM;

namespace Relativity.MassImport.NUnit.Core
{
	[TestFixture]
	public class MassImportMetricsTests
	{
		private const string ExpectedJobStartedMetricName = "Relativity.MassImport.JobStarted";
		private const string ExpectedPreImportStagingTableDetailsMetricName = "Relativity.MassImport.StagingTableDetailsBeforeImport";

		private Mock<IAPM> _apmMock;
		private Mock<ILog> _loggerMock;

		private MassImportMetrics _sut;

		[SetUp]
		public void SetUp()
		{
			_apmMock = new Mock<IAPM>();
			_loggerMock = new Mock<ILog>();

			_sut = new MassImportMetrics(_loggerMock.Object, _apmMock.Object);
		}

		[Test]
		public void ShouldSendJobStartedMetricsForNatives()
		{
			// arrange
			const string importType = "Natives";
			const string system = "UnitTest";

			string runId = Guid.NewGuid().ToString();

			var mappedFields = new[]
			{
				new FieldInfo
				{
					Category = FieldCategory.Generic,
					Type = FieldTypeHelper.FieldType.Decimal
				}
			};

			var settings = new NativeLoadInfo
			{
				RunID = runId,
				MappedFields = mappedFields
			};

			// act
			_sut.SendJobStarted(settings, importType, system);

			// assert
			_apmMock.Verify(x => x.CountOperation(
				ExpectedJobStartedMetricName,
				It.IsAny<Guid>(),
				runId,
				It.IsAny<string>(),
				It.IsAny<bool>(),
				It.IsAny<int?>(),
				It.Is<Dictionary<string, object>>(y => VerifyCustomData(settings, importType, system, y)),
				It.IsAny<IEnumerable<ISink>>()
				));
			_loggerMock.Verify(x => x.LogInformation(
				"Relativity.MassImport metric. Bucket: {bucketName}, type: Counter, value: {@customData}",
				ExpectedJobStartedMetricName,
				It.Is<Dictionary<string, object>>(y => VerifyCustomData(settings, importType, system, y))
				));
		}

		[Test]
		public void ShouldIncludeFieldsDetailsInJobStartedMetricsForNatives()
		{
			// arrange
			const string importType = "Natives";
			const string system = "UnitTest";

			string runId = Guid.NewGuid().ToString();

			var mappedFields = new[]
			{
				new FieldInfo
				{
					Category = FieldCategory.FolderName,
					Type = FieldTypeHelper.FieldType.Varchar
				},
				new FieldInfo
				{
					Type = FieldTypeHelper.FieldType.Varchar,
					EnableDataGrid = true
				},
				new FieldInfo
				{
					Type = FieldTypeHelper.FieldType.Text,
					Category = FieldCategory.FullText,
					EnableDataGrid = true
				},
				new FieldInfo
				{
					Type = FieldTypeHelper.FieldType.Object,
				},
				new FieldInfo
				{
					Type = FieldTypeHelper.FieldType.Object,
				},
				new FieldInfo
				{
					Type = FieldTypeHelper.FieldType.Object,
				},
				new FieldInfo
				{
					Type = FieldTypeHelper.FieldType.Objects,
				},
				new FieldInfo
				{
					Type = FieldTypeHelper.FieldType.Code,
				},
				new FieldInfo
				{
					Type = FieldTypeHelper.FieldType.Code,
				},
			};

			var settings = new NativeLoadInfo
			{
				RunID = runId,
				MappedFields = mappedFields
			};

			var expectedFieldsDetails = new Dictionary<string, object>()
			{
				["NumberOfFullTextFields"] = 1,
				["NumberOfDataGridFields"] = 2,
				["NumberOfOffTableTextFields"] = 0,
				["NumberOfSingleObjectFields"] = 3,
				["NumberOfMultiObjectFields"] = 1,
				["NumberOfSingleChoiceFields"] = 2,
				["NumberOfMultiChoiceFields"] = 0,
				["IsFolderNameMapped"] = true,
			};

			// act
			_sut.SendJobStarted(settings, importType, system);

			// assert
			_apmMock.Verify(x => x.CountOperation(
				ExpectedJobStartedMetricName,
				It.IsAny<Guid>(),
				runId,
				It.IsAny<string>(),
				It.IsAny<bool>(),
				It.IsAny<int?>(),
				It.Is<Dictionary<string, object>>(y => VerifyCustomData(expectedFieldsDetails, y)),
				It.IsAny<IEnumerable<ISink>>()
			));
			_loggerMock.Verify(x => x.LogInformation(
				"Relativity.MassImport metric. Bucket: {bucketName}, type: Counter, value: {@customData}",
				ExpectedJobStartedMetricName,
				It.Is<Dictionary<string, object>>(y => VerifyCustomData(expectedFieldsDetails, y))
			));
		}

		[Test]
		public void ShouldSendJobStartedMetricsForObjects()
		{
			// arrange
			const string importType = "Objects";
			const string system = "UnitTest";

			string runId = Guid.NewGuid().ToString();

			var mappedFields = new[]
			{
				new FieldInfo
				{
					Category = FieldCategory.Generic,
					Type = FieldTypeHelper.FieldType.Decimal
				}
			};

			var settings = new ObjectLoadInfo
			{
				RunID = runId,
				MappedFields = mappedFields,
				ArtifactTypeID = 7
			};

			// act
			_sut.SendJobStarted(settings, importType, system);

			// assert
			_apmMock.Verify(x => x.CountOperation(
				ExpectedJobStartedMetricName,
				It.IsAny<Guid>(),
				runId,
				It.IsAny<string>(),
				It.IsAny<bool>(),
				It.IsAny<int?>(),
				It.Is<Dictionary<string, object>>(y => VerifyCustomData(settings, importType, system, y)),
				It.IsAny<IEnumerable<ISink>>()
			));
			_loggerMock.Verify(x => x.LogInformation(
				"Relativity.MassImport metric. Bucket: {bucketName}, type: Counter, value: {@customData}",
				ExpectedJobStartedMetricName,
				It.Is<Dictionary<string, object>>(y => VerifyCustomData(settings, importType, system, y))
			));
		}

		[Test]
		public void ShouldSendJobStartedMetricsForImages()
		{
			// arrange
			const string importType = "Images";
			const string system = "UnitTest";

			string runId = Guid.NewGuid().ToString();

			var settings = new Relativity.MassImport.DTO.ImageLoadInfo
			{
				RunID = runId,
			};

			// act
			_sut.SendJobStarted(settings, importType, system);

			// assert
			_apmMock.Verify(x => x.CountOperation(
				ExpectedJobStartedMetricName,
				It.IsAny<Guid>(),
				runId,
				It.IsAny<string>(),
				It.IsAny<bool>(),
				It.IsAny<int?>(),
				It.Is<Dictionary<string, object>>(y => VerifyCustomData(settings, importType, system, y)),
				It.IsAny<IEnumerable<ISink>>()
			));
			_loggerMock.Verify(x => x.LogInformation(
				"Relativity.MassImport metric. Bucket: {bucketName}, type: Counter, value: {@customData}",
				ExpectedJobStartedMetricName,
				It.Is<Dictionary<string, object>>(y => VerifyCustomData(settings, importType, system, y))
			));
		}

		[Test]
		public void ShouldSendSendPreImportStagingTableStatistics()
		{
			// arrange
			string runId = Guid.NewGuid().ToString();
			
			var customData = new Dictionary<string, object>
			{
				["NumberOfChoices"] = 7 + 2 + 321,
				["NumberOfChoices_1"] = 7,
				["NumberOfChoices_43"] = 2,
				["NumberOfChoices_98"] = 321
			};

			// act
			_sut.SendPreImportStagingTableStatistics(runId, customData);

			// assert
			_apmMock.Verify(x => x.CountOperation(
				ExpectedPreImportStagingTableDetailsMetricName,
				It.IsAny<Guid>(),
				runId,
				It.IsAny<string>(),
				It.IsAny<bool>(),
				It.IsAny<int?>(),
				It.Is<Dictionary<string, object>>(y => VerifyCustomData(customData, y)),
				It.IsAny<IEnumerable<ISink>>()
			));
			_loggerMock.Verify(x => x.LogInformation(
				"Relativity.MassImport metric. Bucket: {bucketName}, type: Counter, value: {@customData}",
				ExpectedPreImportStagingTableDetailsMetricName,
				It.Is<Dictionary<string, object>>(y => VerifyCustomData(customData, y))
			));
		}

		public bool VerifyCustomData(
			NativeLoadInfo settings,
			string importType,
			string system,
			Dictionary<string, object> customData)
		{
			VerifyCommonCustomData(importType, system, customData);

			string[] propertiesNotIncludedInCustomData =
			{
				nameof(NativeLoadInfo.RunID),
				nameof(NativeLoadInfo.HaveDataGridFields),
				nameof(NativeLoadInfo.HasDataGridWorkToDo),
				nameof(NativeLoadInfo.KeyFieldColumnName),
				nameof(NativeLoadInfo.Repository),
				nameof(NativeLoadInfo.DataFileName),
				nameof(NativeLoadInfo.CodeFileName),
				nameof(NativeLoadInfo.ObjectFileName),
				nameof(NativeLoadInfo.DataGridFileName),
				nameof(NativeLoadInfo.DataGridOffsetFileName),
				nameof(NativeLoadInfo.BulkLoadFileFieldDelimiter),
				nameof(NativeLoadInfo.OnBehalfOfUserToken),
				nameof(NativeLoadInfo.Range),
				nameof(NativeLoadInfo.MappedFields),
			};

			VerifyAllPropertiesAreIncluded(settings, propertiesNotIncludedInCustomData, customData);

			// range
			AssertThatDictionaryContainsKeyValuePair(customData, "RangeDefined", settings.Range != null);
			AssertThatDictionaryContainsKeyValuePair(customData, "RangeStart", settings.Range?.StartIndex);
			AssertThatDictionaryContainsKeyValuePair(customData, "RangeCount", settings.Range?.Count);

			// fields details, assertions only for key
			Assert.That(customData, Contains.Key("NumberOfFullTextFields"));
			Assert.That(customData, Contains.Key("NumberOfDataGridFields"));
			Assert.That(customData, Contains.Key("NumberOfOffTableTextFields"));
			Assert.That(customData, Contains.Key("NumberOfSingleObjectFields"));
			Assert.That(customData, Contains.Key("NumberOfMultiObjectFields"));
			Assert.That(customData, Contains.Key("NumberOfSingleChoiceFields"));
			Assert.That(customData, Contains.Key("NumberOfMultiChoiceFields"));
			Assert.That(customData, Contains.Key("IsFolderNameMapped"));

			return true;
		}

		public bool VerifyCustomData(
			Relativity.MassImport.DTO.ImageLoadInfo settings,
			string importType,
			string system,
			Dictionary<string, object> customData)
		{
			VerifyCommonCustomData(importType, system, customData);

			string[] propertiesNotIncludedInCustomData =
			{
				nameof(Relativity.MassImport.DTO.ImageLoadInfo.RunID),
				nameof(Relativity.MassImport.DTO.ImageLoadInfo.Repository),
				nameof(Relativity.MassImport.DTO.ImageLoadInfo.BulkFileName),
				nameof(Relativity.MassImport.DTO.ImageLoadInfo.DataGridFileName),
			};

			VerifyAllPropertiesAreIncluded(settings, propertiesNotIncludedInCustomData, customData);

			return true;
		}

		private bool VerifyCustomData(
			ObjectLoadInfo settings,
			string importType,
			string system,
			Dictionary<string, object> customData)
		{
			VerifyCustomData(settings as NativeLoadInfo, importType, system, customData);

			AssertThatDictionaryContainsKeyValuePair(customData, nameof(settings.ArtifactTypeID), settings.ArtifactTypeID);
			return true;
		}

		private bool VerifyCustomData(
			Dictionary<string, object> expectedData,
			Dictionary<string, object> customData)
		{
			foreach (var expectedDataKey in expectedData.Keys)
			{
				AssertThatDictionaryContainsKeyValuePair(customData, expectedDataKey, expectedData[expectedDataKey]);
			}
			return true;
		}

		private void VerifyCommonCustomData(string importType, string system, Dictionary<string, object> customData)
		{
			AssertThatDictionaryContainsKeyValuePair(customData, "ImportType", importType);
			AssertThatDictionaryContainsKeyValuePair(customData, "System", system);
			AssertThatDictionaryContainsKeyValuePair(customData, "MassImportImprovementsToggle", true);
		}

		private void VerifyAllPropertiesAreIncluded<T>(T settings, string[] propertiesNotIncludedInCustomData, Dictionary<string, object> customData)
		{
			foreach (PropertyInfo property in typeof(T).GetProperties())
			{
				if (propertiesNotIncludedInCustomData.Contains(property.Name))
				{
					continue;
				}

				object value = property.GetValue(settings);
				if (property.PropertyType.IsEnum)
				{
					value = value.ToString(); // all enums must be converted to a string in order to be serialized correctly.
				}

				AssertThatDictionaryContainsKeyValuePair(customData, property.Name, value);
			}
		}

		private void AssertThatDictionaryContainsKeyValuePair<TKey, TValue>(
			Dictionary<TKey, TValue> dictionary,
			TKey key,
			TValue value)
		{
			Assert.That(dictionary, Contains.Key(key));
			Assert.That(dictionary[key], Is.EqualTo(value), $"Wrong value for '{key}' key.");
		}
	}
}
