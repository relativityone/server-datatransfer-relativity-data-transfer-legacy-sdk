// <copyright file="IMetricsPublisher.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Relativity.DataTransfer.Legacy.Services.Metrics
{
	/// <summary> 
	/// Publish metrics to any sink. 
	/// </summary> 
	public interface IMetricsPublisher
	{
		/// <summary> 
		/// Publish metrics to any sink. 
		/// </summary> 
		/// <param name="metrics"></param> 
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns> 
		Task Publish(Dictionary<string, object> metrics);
	}
}