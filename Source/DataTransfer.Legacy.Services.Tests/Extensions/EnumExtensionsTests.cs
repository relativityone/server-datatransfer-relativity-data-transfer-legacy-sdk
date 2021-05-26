using System;
using FluentAssertions;
using NUnit.Framework;
using Relativity.DataTransfer.Legacy.Services.Extensions;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Extensions
{
	[TestFixture]
	public class EnumExtensionsTests
	{
		[Test]
		public void ShouldThrowInvalidOperationExceptionWhenInvokedForClass()
		{
			var sampleClass = new SampleClass();

			FluentActions.Invoking(() => sampleClass.GetDescription())
				.Should().Throw<InvalidOperationException>()
				.WithMessage("The type specified is not an enum type: Relativity.DataTransfer.Legacy.Services.Tests.Extensions.EnumExtensionsTests+SampleClass.");
		}

		[Test]
		public void ShouldReturnEnumValueNameWhenValueHasNoDescriptionAttribute()
		{
			var description = SampleEnumWithNoDescription.Value.GetDescription();

			description.Should().Be("Value");
		}

		[Test]
		public void ShouldReturnDescriptionFromAttributeForEnumValue()
		{
			var description = SampleEnumWithoutDescription.Value.GetDescription();

			description.Should().Be("ValueDescription");
		}

		private class SampleClass
		{
		}

		private enum SampleEnumWithNoDescription
		{
			Value
		}

		private enum SampleEnumWithoutDescription
		{
			[System.ComponentModel.Description("ValueDescription")] Value,
		}
	}
}
