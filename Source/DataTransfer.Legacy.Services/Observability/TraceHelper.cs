// <copyright file="TraceHelper.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

namespace Relativity.DataTransfer.Legacy.Services.Observability
{
	using OpenTelemetry;
	using OpenTelemetry.Context.Propagation;
	using System.Collections.Generic;
	using System.Diagnostics;

	public class TraceHelper
	{
		public static string SerializeContext(Activity activity, Baggage baggage)
		{
			if (activity == null)
			{
				return string.Empty;
			}

			const string traceParentKey = "traceparent";
			var props = new Dictionary<string, string>();

			void Setter(Dictionary<string, string> properties, string key, string value)
			{
				properties[key] = value;
			}

			try
			{
				Propagators.DefaultTextMapPropagator.Inject(new PropagationContext(activity.Context, baggage), props, Setter);
			}
			catch { }

			if (props.ContainsKey(traceParentKey))
			{
				var context = props[traceParentKey];
				return context;
			}
			return string.Empty;
		}

		public static ActivityContext DeserializeContext(string context)
		{
			ActivityContext.TryParse(context, null, out ActivityContext activityContext);
			return activityContext;
		}
	}
}