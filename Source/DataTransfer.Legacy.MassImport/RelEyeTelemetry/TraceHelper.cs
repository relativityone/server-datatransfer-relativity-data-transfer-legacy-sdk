namespace DataTransfer.Legacy.MassImport.RelEyeTelemetry
{
	using System;
	using System.Diagnostics;
	using OpenTelemetry.Trace;

	public class TraceHelper
	{
		/// <summary>
		/// Set status ok.
		/// </summary>
		/// <param name="activity"></param>
		public static void SetStatusOK(Activity activity)
		{
			activity?.SetStatus(ActivityStatusCode.Ok);
		}

		/// <summary>
		/// Set status error.
		/// </summary>
		/// <param name="activity"></param>
		/// <param name="message"></param>
		/// <param name="ex"></param>
		public static void SetStatusError(Activity activity, string message, Exception ex = null)
		{
			activity?.SetStatus(ActivityStatusCode.Error, message);

			// shall send event with exception details if ex != null.
			activity?.RecordException(ex);
		}
	}
}
