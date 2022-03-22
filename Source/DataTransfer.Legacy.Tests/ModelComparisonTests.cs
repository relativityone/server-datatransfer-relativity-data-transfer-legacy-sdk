using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using FluentAssertions;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Tests.Helpers;

namespace Relativity.DataTransfer.Legacy.Tests
{
	[TestFixture]
	public class ModelComparisonTests
	{
		private RandomObjectGenerator _randomObjectGenerator;
		private CompareLogic _compareLogic;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_randomObjectGenerator = new RandomObjectGenerator();
			_compareLogic = new CompareLogic { Config = { IgnoreObjectTypes = true } };
			_compareLogic.Config.CustomComparers.Add(new EnumComparerIgnoringType(RootComparerFactory.GetRootComparer()));
		}

		[Test]
		[TestCaseSource(nameof(GetMappings))]
		[Order(1)]
		public void MakeSureNumberOfPropertiesMatch(TypeMap typeMap)
		{
			var sourceProperties = typeMap.SourceType
				.GetFields(BindingFlags.Public | BindingFlags.Instance).Cast<MemberInfo>()
				.Concat(typeMap.SourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)).ToArray();
			var destinationProperties = typeMap.DestinationType
				.GetFields(BindingFlags.Public | BindingFlags.Instance).Cast<MemberInfo>()
				.Concat(typeMap.DestinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)).ToArray();

			//original ObjectLoadInfo and NativeLoadInfo has three additional getters
			//which return values from different properties based on some conditions - no need to rewrite them
			//NativeLoadInfo model has one more property for bulk import to overwrite the bcp path
			int modifier = 0;
			if (typeMap.SourceType.FullName == typeof(SDK.ImportExport.V1.Models.ObjectLoadInfo).FullName
				|| typeMap.SourceType.FullName == typeof(SDK.ImportExport.V1.Models.NativeLoadInfo).FullName)
			{
				modifier = -2;
			}

			sourceProperties.Length.Should().BeGreaterThan(0, "to make sure we took fields in a correct way");
			sourceProperties.Length.Should().Be(destinationProperties.Length + modifier, "{0} {1}", typeMap.SourceType, typeMap.DestinationType);
		}

		[Test]
		[TestCaseSource(nameof(GetMappings))]
		[Order(2)]
		public void MakeSureAllValuesAreRewritten(TypeMap typeMap)
		{
			object generate = _randomObjectGenerator.Generate(typeMap.SourceType);
			if (generate == null)
			{
				Assert.Fail("Cannot find generator for {0}", typeMap.SourceType);
			}

			object map = ModelExtensions.Mapper.Map(generate, typeMap.SourceType, typeMap.DestinationType);

			ComparisonResult result = _compareLogic.Compare(generate, map);
			result.AreEqual.Should().BeTrue(result.DifferencesString);
		}

		[Test]
		[TestCaseSource(nameof(GetMappings))]
		[Order(3)]
		public void MakeSureDefaultValuesAreEqual(TypeMap typeMap)
		{
			object source = Activator.CreateInstance(typeMap.SourceType);
			object destination = Activator.CreateInstance(typeMap.DestinationType);

			ComparisonResult result = _compareLogic.Compare(source, destination);
			result.AreEqual.Should().BeTrue(result.DifferencesString);
		}

		private static IEnumerable<TypeMap> GetMappings()
		{
			return ModelExtensions.Mapper.ConfigurationProvider.GetAllTypeMaps();
		}
	}
}