// <copyright file="IMetricsContext.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

namespace Relativity.DataTransfer.Legacy.Services.Metrics
{
	using System;
	using System.Diagnostics;

	public interface ITraceGenerator : IDisposable
	{
		ActivitySource GetActivitySurce();
		void SetSystemTags(Activity activity);
	}
}