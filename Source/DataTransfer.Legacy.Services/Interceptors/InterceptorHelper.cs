using System;
using System.Collections.Generic;
using System.Linq;
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
				string value;
				if (parameters[i].ParameterType.IsArray && invocation.Arguments[i] is Array array)
				{
					var sb = new StringBuilder();
					foreach (object element in array)
					{
						sb.Append(element + ";");
					}

					value = sb.ToString();
				}
				else
				{
					value = invocation.Arguments[i]?.ToString();
				}

				value = value ?? "null";

				if (Attribute.IsDefined(parameters[i], typeof(SensitiveDataAttribute)))
				{
					value = HashValue(value);
				}
				arguments.Add(parameters[i].Name, value);
			}

			return arguments;
		}

		/// <summary>
		/// Builds custom error text using exception type and exception message.
		/// When the ServiceException is thrown and developer mode is disabled for environment, the real inner exception is not returned by kepler service,
		/// but in some cases (e.g.: for RDC, IAPI) the inner exception type and message is needed to correctly handle the error.
		/// </summary>
		/// <param name="ex">Exception</param>
		/// <returns>Text based on exception type and message</returns>
		public static string BuildErrorMessageDetails(Exception ex)
		{
			return $"InnerExceptionType: {ex.GetType()}, InnerExceptionMessage: {ex.Message}";
		}

		public static string HashValue(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}
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