using System;
using System.Reflection;

namespace Relativity.DataTransfer.Legacy.Services.Helpers
{
	public static class VersionHelper
	{
		private static readonly Lazy<string> _appVersionLazy = new Lazy<string>(RetrieveServiceVersion);
		private static string RetrieveServiceVersion()
		{
			return Assembly.GetAssembly(typeof(VersionHelper)).GetName().Version.ToString();
		}

		public static string GetVersion()
		{
			return _appVersionLazy.Value;
		}
	}
}
