using System;
using System.Security.Cryptography;
using System.Text;

namespace Relativity.DataTransfer.Legacy.SDK.ImportExport.V1.Models
{
	internal static class ModelExtensions
	{
		public static string ToSafeString(this object o)
		{
			if (o == null)
			{
				return string.Empty;
			}

			var properties = o.GetType().GetProperties();

			var sb = new StringBuilder();

			foreach (var info in properties)
			{
				var value = info.GetValue(o, null) ?? "(null)";

				if (Attribute.IsDefined(info, typeof(SensitiveDataAttribute)))
				{
					value = HashValue(value.ToString());
				}

				sb.AppendLine($"{info.Name}: {value}");
			}

			return sb.ToString();
		}

		private static string HashValue(string value)
		{
			StringBuilder sb = new StringBuilder();
			using (SHA256 hash = SHA256.Create())
			{
				Encoding enc = Encoding.UTF8;
				byte[] result = hash.ComputeHash(enc.GetBytes(value));

				foreach (byte b in result)
				{
					sb.Append(b.ToString("x2"));
				}
			}

			return sb.ToString();
		}
	}
}