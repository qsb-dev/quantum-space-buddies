namespace QSB.ClientServerStateSync
{
	public enum ClientState
	{
		NotLoaded,
		InTitleScreen,
		AliveInSolarSystem,
		DeadInSolarSystem,
		AliveInEye,
		WaitingForOthersToDie,
		WaitingForOthersToBeReady,
		WatchingLongCredits,
		WatchingShortCredits
	}
}
