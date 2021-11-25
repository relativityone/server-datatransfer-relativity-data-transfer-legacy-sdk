using System.Collections.Generic;
using System.Data.SqlTypes;

namespace Relativity.MassImport.Core
{
	internal class CollationStringComparer : IEqualityComparer<string>
	{
		private int lcid;
		private SqlCompareOptions sqlCompareOptions;

		private CollationStringComparer(int lcid, SqlCompareOptions sqlCompareOptions)
		{
			this.lcid = lcid;
			this.sqlCompareOptions = sqlCompareOptions;
		}

		public static CollationStringComparer SQL_Latin1_General_CP1_CI_AS
		{
			get
			{
				return new CollationStringComparer(0x0409, SqlCompareOptions.IgnoreCase | SqlCompareOptions.IgnoreWidth | SqlCompareOptions.IgnoreKanaType);
			}
		}

		public bool Equals(string x, string y)
		{
			var xw = new SqlString(x, lcid, sqlCompareOptions);
			var yw = new SqlString(y, lcid, sqlCompareOptions);
			return xw.CompareTo(yw) == 0;
		}

		public int GetHashCode(string obj)
		{
			var objw = new SqlString(obj, lcid, sqlCompareOptions);
			return objw.GetHashCode();
		}
	}
}