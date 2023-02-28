// <copyright file="ITraceGenerator.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

namespace Relativity.DataTransfer.Legacy.Services.Observability
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;

	public interface ITraceGenerator
	{
		Activity StartActivity(string name, ActivityKind kind, ActivityContext parentContext = default(ActivityContext), IEnumerable<KeyValuePair<string, object>> tags = null, IEnumerable<ActivityLink> links = null, DateTimeOffset startTime = default);
	}
}