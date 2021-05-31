using System;
using System.ComponentModel;
using System.Reflection;

namespace Relativity.DataTransfer.Legacy.Services.Extensions
{
	public static class EnumExtensions
	{
		public static string GetDescription<T>(this T value)
		{
			var type = typeof(T);
			if (!type.IsEnum)
			{
				throw new InvalidOperationException($"The type specified is not an enum type: {type}.");
			}

			var enumName = type.GetEnumName(value);

			var field = type.GetField(enumName);
			var customAttribute = field.GetCustomAttribute(typeof(DescriptionAttribute));

			string description;
			if (customAttribute is DescriptionAttribute attribute)
			{
				description = attribute.Description;
			}
			else
			{
				description = enumName;
			}

			return description;
		}
	}
}
