
namespace Relativity.MassImport.Core.Pipeline.Input.Interface
{
	internal interface IImportSettingsInput<out TSettings>
	{
		TSettings Settings { get; }
	}
}