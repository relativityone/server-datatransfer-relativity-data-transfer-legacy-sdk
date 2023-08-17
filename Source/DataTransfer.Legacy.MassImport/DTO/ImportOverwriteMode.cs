
namespace Relativity.MassImport.DTO
{
	public enum OverwriteType
	{
		Append,
		Overlay,
		Both
	}

	public class OverwriteTypeHelper
	{
		public static string ConvertToDisplayString(OverwriteType mode)
		{
			switch (mode)
			{
				case OverwriteType.Append:
					{
						return "Append Only";
					}

				case OverwriteType.Overlay:
					{
						return "Overlay Only";  // Both
					}

				default:
					{
						return "Append/Overlay";
					}
			}
		}
	}
}