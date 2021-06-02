// <copyright file="LogInterceptor.cs" company="Relativity ODA LLC">
// © Relativity All Rights Reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport;

namespace Relativity.DataTransfer.Legacy.Services.Interceptors
{
	/// <summary>
	/// Read and log all useful information from intercepted method context.
	/// </summary>
	public class LogInterceptor : InterceptorBase
	{
		private readonly IAPILog _logger;
		private List<IDisposable> _contextPushPropertiesHandlers;

		/// <summary>
		/// Initializes a new instance of the <see cref="LogInterceptor"/> class.
		/// </summary>
		/// <param name="logger">Logger.</param>
		public LogInterceptor(IAPILog logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc />
		public override void ExecuteBefore(IInvocation invocation)
		{
			const string Controller = "Controller";
			const string EndpointCalled = "EndpointCalled";
			Dictionary<string, string> arguments = GetFunctionAttributes(invocation);

			_contextPushPropertiesHandlers = new List<IDisposable>
			{
				_logger.LogContextPushProperty(Controller, invocation.TargetType.Name),
				_logger.LogContextPushProperty(EndpointCalled, invocation.Method.Name)
			};

			_contextPushPropertiesHandlers.AddRange(arguments.Select(argument => _logger.LogContextPushProperty(argument.Key, argument.Value)));
		}

		/// <inheritdoc />
		public override Task ExecuteAfter(IInvocation invocation, dynamic returnValue)
		{
			DisposeLoggerContext(_contextPushPropertiesHandlers);
			return Task.CompletedTask;
		}

		private static Dictionary<string, string> GetFunctionAttributes(IInvocation invocation)
		{
			Type type = Type.GetType($"{invocation.TargetType.FullName}, {invocation.TargetType.Assembly.FullName}");
			Dictionary<string, string> arguments = new Dictionary<string, string>();
			if (type == null)
			{
				return arguments;
			}

			ParameterInfo[] parameters = invocation.Method.GetParameters();
			if (parameters.Length != invocation.Arguments.Length)
			{
				return arguments;
			}

			for (int i = 0; i < parameters.Length; i++)
			{
				string value = invocation.Arguments[i]?.ToString() ?? "null";
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
			StringBuilder sb = new StringBuilder();
			using (SHA256 hash = SHA256.Create())
			{
				Encoding enc = Encoding.UTF8;
				Byte[] result = hash.ComputeHash(enc.GetBytes(value));

				foreach (Byte b in result)
				{
					sb.Append(b.ToString("x2"));
				}
			}
			return sb.ToString();
		}

		private static void DisposeLoggerContext(List<IDisposable> loggers)
		{
			foreach (var handler in loggers.Where(x => x != null))
			{
				handler.Dispose();
			}
		}
	}
}