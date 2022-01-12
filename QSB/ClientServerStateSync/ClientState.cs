namespace QSB.ClientServerStateSync
{
	public enum ClientState
	{
		NotLoaded,
		InTitleScreen,
		AliveInSolarSystem,
		DeadInSolarSystem,
		AliveInEye,
		WaitingForOthersToBeReady,
		WatchingLongCredits,
		WatchingShortCredits
	}
}
