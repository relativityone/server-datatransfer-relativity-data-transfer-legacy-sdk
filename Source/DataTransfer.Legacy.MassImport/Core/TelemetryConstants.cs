// <copyright file="TelemetryConstants.cs" company="Relativity ODA LLC"> 
// © Relativity All Rights Reserved. 
// </copyright> 

namespace DataTransfer.Legacy.MassImport.Core
{
	public class TelemetryConstants
	{
		public class MetricsAttributes
		{
			/// <summary>
			/// Team ID attribute.
			/// </summary>
			public const string OwnerTeamId = "r1.team.id";
			public const string SystemName = "service.namespace";
			public const string ServiceName = "service.name";
			public const string ApplicationID = "application.guid";
			public const string ApplicationName = "application.name";
		}

		public class RelEyeSettings
		{
			public const string RelativityTelemetrySection = "Relativity.Telemetry";
			public const string ReleyeUriTracesSettingName = "ReleyeUriTraces";
			public const string ReleyeTokenSettingName = "ReleyeToken";
		}

		public class Application
		{
			/// <summary>
			///  ID of 'Holy Data Acquisition' team.
			/// </summary>
			public const string OwnerTeamId = "PTCI-4941411";
			public const string SystemName = "data-transfer-legacy-rap";
			public const string ServiceName = "data-transfer-legacy-rap-kepler-api";
			public const string ApplicationID = "9f9d45ff-5dcd-462d-996d-b9033ea8cfce";
			public const string ApplicationName = "DataTranfer.Legacy";
		}
	}
}
