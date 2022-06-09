
namespace Relativity.MassImport.DTO
{
	public enum OverlayBehavior
	{
		UseRelativityDefaults,
		MergeAll,
		ReplaceAll
	}

	public class OverlayBehaviorHelper
	{
		public static string ConvertToDisplayString(OverlayBehavior? mode)
		{
			if (mode is null)
				return "";
			switch (mode)
			{
				case OverlayBehavior.UseRelativityDefaults:
					{
						return "Use Field Settings";
					}

				case OverlayBehavior.MergeAll:
					{
						return "Merge Values";  // old school replace
					}

				default:
					{
						return "Replace Values";
					}
			}
		}
	}
}