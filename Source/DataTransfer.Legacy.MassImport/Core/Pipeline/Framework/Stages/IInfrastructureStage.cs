
namespace Relativity.MassImport.Core.Pipeline.Framework.Stages
{
	/// <summary>
	/// This interface is used to mark node as an infrastructure node, so executor can apply different logic.
	/// </summary>
	internal interface IInfrastructureStage : Stages.ICustomJsonSerializationStage
	{
	}
}