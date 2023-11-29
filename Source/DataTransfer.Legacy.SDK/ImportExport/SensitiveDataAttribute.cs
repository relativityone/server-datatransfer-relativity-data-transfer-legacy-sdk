using System;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport
{
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
	public sealed class SensitiveDataAttribute : Attribute
	{
	}
}