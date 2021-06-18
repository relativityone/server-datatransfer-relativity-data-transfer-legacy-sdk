// <copyright file="APMMetricsPublisher.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.Telemetry.APM;

namespace Relativity.DataTransfer.Legacy.Services.Metrics
{
	/// <summary> 
	/// Publish metrics to New Relic. 
	/// </summary> 
	public class APMMetricsPublisher : IMetricsPublisher
	{
		private const string BucketName = "DataTransfer.Legacy.KeplerCall";

		private readonly IAPM _apm;

		/// <summary> 
		/// Initializes a new instance of the <see cref="APMMetricsPublisher"/> class. 
		/// </summary> 
		/// <param name="apm"></param> 
		public APMMetricsPublisher(IAPM apm)
		{
			_apm = apm;
		}

		/// <summary> 
		/// Publish metrics to New Relic. 
		/// </summary> 
		/// <param name="metrics"></param> 
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns> 
		public Task Publish(Dictionary<string, object> metrics)
		{
			_apm.CountOperation(BucketName, customData: metrics);
			return Task.CompletedTask;
		}
	}
}