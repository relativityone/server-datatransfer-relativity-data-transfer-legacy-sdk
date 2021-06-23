using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Castle.DynamicProxy;
using Relativity.DataTransfer.Legacy.SDK.ImportExport;

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
				if (Attribute.IsDefined(parameters[i], typeof(SensitiveDataAttribute)))
				{
					value = HashValue(value);
				}
				arguments.Add(parameters[i].Name, value);
			}

			return arguments;
		}

		private static string HashValue(string value)
		{
			var sb = new StringBuilder();
			using (var hash = SHA256.Create())
			{
				var enc = Encoding.UTF8;
				var result = hash.ComputeHash(enc.GetBytes(value));

				foreach (var b in result)
				{
					sb.Append(b.ToString("x2"));
				}
			}
			return sb.ToString();
		}
	}
}