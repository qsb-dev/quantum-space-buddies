namespace QSB.ClientServerStateSync
{
	public enum ServerState
	{
		// When in menus
		NotLoaded,

		// When in any credits
		Credits,

		// For normal play in SolarSystem
		InSolarSystem,

		// For normal play in EyeOfTheUniverse
		InEye,

		// At end of loop, waiting for everyone to be ready to reload the scene
		WaitingForAllPlayersToDie,

		// At start of loop, waiting for everybody to be ready to start playing
		WaitingForAllPlayersToReady,

		// When the statue has been activated
		InStatueCutscene
	}
}