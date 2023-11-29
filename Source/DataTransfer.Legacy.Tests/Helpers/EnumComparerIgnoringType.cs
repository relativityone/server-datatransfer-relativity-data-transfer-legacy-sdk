using System;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;

namespace Relativity.DataTransfer.Legacy.Tests.Helpers
{
	public class EnumComparerIgnoringType : EnumComparer
	{
		public EnumComparerIgnoringType(RootComparer rootComparer) : base(rootComparer)
		{
		}

		public override bool IsTypeMatch(Type type1, Type type2)
		{
			return type1 != null && type1.IsEnum;
		}

		public override void CompareType(CompareParms @params)
		{
			if (@params.Object1 == null || @params.Object2 == null)
			{
				if (@params.Object1 != @params.Object2)
				{
					AddDifference(@params);
				}
			}
			else if ((int) @params.Object1 != (int) @params.Object2)
			{
				AddDifference(@params);
			}
		}
	}
}