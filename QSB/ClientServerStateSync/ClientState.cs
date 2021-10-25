namespace QSB.ClientServerStateSync
{
	public enum ClientState
	{
		NotLoaded,
		InTitleScreen,
		AliveInSolarSystem,
		DeadInSolarSystem,
		AliveInEye,
		WaitingForOthersToDieInSolarSystem,
		WaitingForOthersToReadyInSolarSystem,
		WatchingLongCredits,
		WatchingShortCredits
	}
}
