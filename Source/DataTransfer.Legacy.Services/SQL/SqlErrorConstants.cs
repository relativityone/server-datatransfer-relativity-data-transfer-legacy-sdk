namespace Relativity.DataTransfer.Legacy.Services.SQL
{
	public static class SqlErrorConstants
	{
		public const int ConnectionError = -1;
		public const int ConnectionTimeout = 2;
		public const int NetworkPathNotFound = 53;
		public const int SpecifiedNetworkNameNoLongerAvailable = 64;
		public const int SessionInKillState = 596;
		public const int DatabaseCannotAutoStartDuringShutdownOrStartup = 904;
		public const int DatabaseNameDoesNotExist = 911;
		public const int DatabaseNotFoundByInternalId = 913;
		public const int DatabaseOccupiedInSingleUserMode = 924;
		public const int DatabaseCannotBeOpenedMarkedSuspect = 926;
		public const int DatabaseInRestoreProcess = 927;
		public const int DatabaseOffline = 942;
		public const int DatabaseInTransition = 952;
		public const int DatabaseReplicaNotInCorrectRole = 983;
		public const int NoHighAvailabilityNodeQuorum = 988;
		public const int Deadlocked = 1205;
		public const int UserNameOrPasswordIncorrect = 1326;
		public const int DatabaseSnapshotCannotBeCreated = 1823;
		public const int UserSessionStateHasChanged = 4021;
		public const int CannotOpenUserDefaultDatabase = 4064;
		public const int CannotChangeState = 5064;
		public const int ShutdownInProgress = 6005;
		public const int ExistingConnectionForciblyClosedByRemoteHost = 10054;
		public const int UserAccountIsDisabled = 18470;

		public const string InternalConnectionFatalError = "Internal connection fatal error";
		public const string ConnectionClosedError = "The connection is closed";
		public const string ConnectionNotAvailableError = "requires an open and available Connection";
	}
}
