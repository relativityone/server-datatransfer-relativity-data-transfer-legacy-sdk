using System;
using System.Collections.Generic;
using Castle.DynamicProxy;

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	public class InterceptorHelper
	{
		public static Dictionary<string, string> GetFunctionArgumentsFrom(IInvocation invocation)
		{
			var type = Type.GetType($"{invocation.TargetType.FullName}, {invocation.TargetType.Assembly.FullName}");
			var arguments = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			if (type == null)
			{
				return arguments;
			}

			var parameters = invocation.Method.GetParameters();
			if (parameters.Length != invocation.Arguments.Length)
			{
				return arguments;
			}

			for (var i = 0; i < parameters.Length; i++)
			{
				var value = invocation.Arguments[i]?.ToString() ?? "null";
				arguments.Add(parameters[i].Name, value);
			}

			return arguments;
		}
	}
}