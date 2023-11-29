using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.PostInstallEventHandler
{
	public interface IInstanceSettingsService
	{
		/// <summary>
		/// Creates instance setting of type text.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="section"></param>
		/// <param name="value"></param>
		/// <param name="description"></param>
		/// <returns>Success true/false.</returns>
		Task<bool> CreateInstanceSettingsTextType(string name, string section, string value, string description);
	}
}