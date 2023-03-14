using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Relativity.MassImport.Core;

namespace Relativity.MassImport.NUnit.Core
{
    [TestFixture]
    public class MetricCustomDataBuilderTests
    {
        [Test]
        public void ShouldBuildCustomDataWithChoicesDetails()
        {
            // arrange
            var codeTypeToNumberOfChoicesMapping = new Dictionary<int, int>
            {
                [1] = 7,
                [43] = 2,
                [98] = 321
            };

            // act
            var actualCustomData = MetricCustomDataBuilder
                .New()
                .WithChoicesDetails(codeTypeToNumberOfChoicesMapping)
                .Build();

            // assert
            var expectedCustomData = new Dictionary<string, object>
            {
                ["NumberOfChoices"] = 7 + 2 + 321,
                ["NumberOfChoices_1"] = 7,
                ["NumberOfChoices_43"] = 2,
                ["NumberOfChoices_98"] = 321
            };

            Assert.That(actualCustomData, Is.EquivalentTo(expectedCustomData));
        }

        [Test]
        public void ShouldBuildCustomDataWithChoicesDetailsForEmptyInput()
        {
            // arrange
            var codeTypeToNumberOfChoicesMapping = new Dictionary<int, int>();

            // act
            var actualCustomData = MetricCustomDataBuilder
                .New()
                .WithChoicesDetails(codeTypeToNumberOfChoicesMapping)
                .Build();

            // assert
            var expectedCustomData = new Dictionary<string, object>
            {
                ["NumberOfChoices"] = 0
            };

            Assert.That(actualCustomData, Is.EquivalentTo(expectedCustomData));
        }

        [Test]
        public void ShouldBuildCustomDataWithContext()
        {
            // arrange
            const string importType = "Natives";
            const string system = "UnitTest";

            // act
            var actualCustomData = MetricCustomDataBuilder
                .New()
                .WithContext(importType, system)
                .Build();

            // assert
            VerifyContextInformationInCustomData(importType, system, actualCustomData);
        }

        [Test]
        public void ShouldBuildCustomDataWithNativeSettings()
        {
            // arrange
            string runId = Guid.NewGuid().ToString();

            var mappedFields = new[]
            {
                new FieldInfo
                {
                    Category = FieldCategory.Generic,
                    Type = FieldTypeHelper.FieldType.Decimal
                }
            };

            var settings = new Relativity.MassImport.DTO.NativeLoadInfo
            {
                RunID = runId,
                MappedFields = mappedFields
            };

            // act
            var actualCustomData = MetricCustomDataBuilder
                .New()
                .WithSettings(settings)
                .Build();

            // assert
            VerifySettingsInCustomData(settings, actualCustomData);
        }

        [Test]
        public void ShouldBuildCustomDataWithNativeSettingsAndContext()
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

            var settings = new Relativity.MassImport.DTO.NativeLoadInfo
            {
                RunID = runId,
                MappedFields = mappedFields
            };

            // act
            var actualCustomData = MetricCustomDataBuilder
                .New()
                .WithContext(importType, system)
                .WithSettings(settings)
                .Build();

            // assert
            VerifyContextInformationInCustomData(importType, system, actualCustomData);
            VerifySettingsInCustomData(settings, actualCustomData);
        }

        [Test]
        public void ShouldBuildCustomDataWithFieldsDetails()
        {
            // arrange
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

            var settings = new Relativity.MassImport.DTO.NativeLoadInfo
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
            var actualCustomData = MetricCustomDataBuilder
                .New()
                .WithSettings(settings)
                .Build();

            // assert
            foreach (var expectedDataKey in expectedFieldsDetails.Keys)
            {
                AssertThatDictionaryContainsKeyValuePair(actualCustomData, expectedDataKey, expectedFieldsDetails[expectedDataKey]);
            }
        }

        [Test]
        public void ShouldBuildCustomDataWithObjectsSettings()
        {
            // arrange
            string runId = Guid.NewGuid().ToString();

            var mappedFields = new[]
            {
                new FieldInfo
                {
                    Category = FieldCategory.Generic,
                    Type = FieldTypeHelper.FieldType.Decimal
                }
            };

            var settings = new Relativity.MassImport.DTO.ObjectLoadInfo
            {
                RunID = runId,
                MappedFields = mappedFields,
                ArtifactTypeID = 7
            };

            // act
            var actualCustomData = MetricCustomDataBuilder
                .New()
                .WithSettings(settings)
                .Build();

            // assert
            VerifySettingsInCustomData(settings, actualCustomData);
        }

        [Test]
        public void ShouldBuildCustomDataWithImagesSettings()
        {
            // arrange
            string runId = Guid.NewGuid().ToString();

            var settings = new Relativity.MassImport.DTO.ImageLoadInfo
            {
                RunID = runId,
            };

            // act
            var actualCustomData = MetricCustomDataBuilder
                .New()
                .WithSettings(settings)
                .Build();

            // assert
            VerifySettingsInCustomData(settings, actualCustomData);
        }

        [Test]
        public void WithFieldInfo_AddFieldDetailsToCustomData()
        {
            // arrange
            var fieldInfo = new FieldInfo
            {
                ArtifactID = 7,
                Category = FieldCategory.AutoCreate,
                CodeTypeID = 42,
                DisplayName = "name",
                EnableDataGrid = true,
                FormatString = "format",
                ImportBehavior = FieldInfo.ImportBehaviorChoice.ChoiceFieldIgnoreDuplicates,
                IsUnicodeEnabled = false,
                TextLength = 9,
                Type = FieldTypeHelper.FieldType.MultiCode,
            };

            // act
            var actualCustomData = MetricCustomDataBuilder.New().WithFieldInfo(fieldInfo).Build();

            // assert
            VerifyAllPropertiesAreIncluded(fieldInfo, new[] { nameof(FieldInfo.FormatString) }, actualCustomData, shouldConvertAllPropertiesToString: true);
        }

        private void VerifySettingsInCustomData(Relativity.MassImport.DTO.NativeLoadInfo settings, Dictionary<string, object> customData)
        {
            string[] propertiesNotIncludedInCustomData =
            {
                nameof(Relativity.MassImport.DTO.NativeLoadInfo.RunID),
                nameof(Relativity.MassImport.DTO.NativeLoadInfo.HasDataGridWorkToDo),
                nameof(Relativity.MassImport.DTO.NativeLoadInfo.KeyFieldColumnName),
                nameof(Relativity.MassImport.DTO.NativeLoadInfo.Repository),
                nameof(Relativity.MassImport.DTO.NativeLoadInfo.DataFileName),
                nameof(Relativity.MassImport.DTO.NativeLoadInfo.CodeFileName),
                nameof(Relativity.MassImport.DTO.NativeLoadInfo.ObjectFileName),
                nameof(Relativity.MassImport.DTO.NativeLoadInfo.DataGridFileName),
                nameof(Relativity.MassImport.DTO.NativeLoadInfo.DataGridOffsetFileName),
                nameof(Relativity.MassImport.DTO.NativeLoadInfo.BulkLoadFileFieldDelimiter),
                nameof(Relativity.MassImport.DTO.NativeLoadInfo.OnBehalfOfUserToken),
                nameof(Relativity.MassImport.DTO.NativeLoadInfo.Range),
                nameof(Relativity.MassImport.DTO.NativeLoadInfo.MappedFields),
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
        }

        private void VerifySettingsInCustomData(Relativity.MassImport.DTO.ObjectLoadInfo settings, Dictionary<string, object> customData)
        {
            VerifySettingsInCustomData(settings as Relativity.MassImport.DTO.NativeLoadInfo, customData);

            AssertThatDictionaryContainsKeyValuePair(customData, nameof(settings.ArtifactTypeID), settings.ArtifactTypeID);
        }

        private void VerifySettingsInCustomData(Relativity.MassImport.DTO.ImageLoadInfo settings, Dictionary<string, object> customData)
        {
            string[] propertiesNotIncludedInCustomData =
            {
                nameof(Relativity.MassImport.DTO.ImageLoadInfo.RunID),
                nameof(Relativity.MassImport.DTO.ImageLoadInfo.Repository),
                nameof(Relativity.MassImport.DTO.ImageLoadInfo.BulkFileName),
                nameof(Relativity.MassImport.DTO.ImageLoadInfo.DataGridFileName),
            };

            VerifyAllPropertiesAreIncluded(settings, propertiesNotIncludedInCustomData, customData);
        }

        private void VerifyContextInformationInCustomData(string importType, string system, Dictionary<string, object> customData)
        {
            AssertThatDictionaryContainsKeyValuePair(customData, "ImportType", importType);
            AssertThatDictionaryContainsKeyValuePair(customData, "System", system);
            AssertThatDictionaryContainsKeyValuePair(customData, "r1.team.id", "PTCI-4941411");
        }

        private void VerifyAllPropertiesAreIncluded<T>(
            T settings,
            string[] propertiesNotIncludedInCustomData,
            Dictionary<string, object> customData,
            bool shouldConvertAllPropertiesToString = false)
        {
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                if (propertiesNotIncludedInCustomData.Contains(property.Name))
                {
                    continue;
                }

                object value = property.GetValue(settings);
                if (shouldConvertAllPropertiesToString || property.PropertyType.IsEnum)
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
