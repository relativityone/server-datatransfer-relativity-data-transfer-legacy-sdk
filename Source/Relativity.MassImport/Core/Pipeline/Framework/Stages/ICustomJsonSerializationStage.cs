
namespace Relativity.MassImport.Core.Pipeline.Framework.Stages
{
	/// <summary>
	/// This interface is used to mark node which implements custom serialization logic.
	/// </summary>
	/// <remarks>Json serialization is used only for debugging purpose.</remarks>
	internal interface ICustomJsonSerializationStage
	{
		string ToString();
	}
}