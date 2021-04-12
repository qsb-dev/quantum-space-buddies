namespace QSB.Events
{
	public static class EventNames
	{
		// Built into Outer Wilds -- don't change unless they change in-game!
		public static string TurnOnFlashlight = "TurnOnFlashlight";
		public static string TurnOffFlashlight = "TurnOffFlashlight";
		public static string LaunchProbe = "LaunchProbe";
		public static string RetrieveProbe = "RetrieveProbe";
		public static string ProbeLauncherEquipped = "ProbeLauncherEquipped";
		public static string ProbeLauncherUnequipped = "ProbeLauncherUnequipped";
		public static string EquipSignalscope = "EquipSignalscope";
		public static string UnequipSignalscope = "UnequipSignalscope";
		public static string SuitUp = "SuitUp";
		public static string RemoveSuit = "RemoveSuit";
		public static string EquipTranslator = "EquipTranslator";
		public static string UnequipTranslator = "UnequipTranslator";
		public static string RestartTimeLoop = "RestartTimeLoop";
		public static string WakeUp = "WakeUp";
		public static string DialogueCondition = "DialogueConditionChanged";
		public static string EnterQuantumMoon = "PlayerEnterQuantumMoon";
		public static string ExitQuantumMoon = "PlayerExitQuantumMoon";
		public static string EnterRoastingMode = "EnterRoastingMode";
		public static string ExitRoastingMode = "ExitRoastingMode";
		public static string EnterFlightConsole = "EnterFlightConsole";
		public static string ExitFlightConsole = "ExitFlightConsole";

		// Custom event names -- change if you want! These can be anything, as long as both
		// sides of the GlobalMessenger (fireevent and addlistener) reference the same thing.
		public static string QSBPlayerDeath = "QSBPlayerDeath";
		public static string QSBPlayerJoin = "QSBPlayerJoin";
		public static string QSBPlayerReady = "QSBPlayerReady";
		public static string QSBSectorChange = "QSBSectorChange";
		public static string QSBPlayerStatesRequest = "QSBPlayerStatesRequest";
		public static string QSBServerTime = "QSBServerTime";
		public static string QSBStartLift = "QSBStartLift";
		public static string QSBGeyserState = "QSBGeyserState";
		public static string QSBCrouch = "QSBAnimTrigger";
		public static string QSBOrbSlot = "QSBOrbSlot";
		public static string QSBOrbUser = "QSBOrbUser";
		public static string QSBConversation = "QSBConversation";
		public static string QSBConversationStartEnd = "QSBConversationStartEnd";
		public static string QSBChangeAnimType = "QSBPlayInstrument";
		public static string QSBServerSendPlayerStates = "QSBServerSendPlayerStates";
		public static string QSBRevealFact = "QSBRevealFact";
		public static string QSBSocketStateChange = "QSBSocketStateChange";
		public static string QSBMultiStateChange = "QSBMultiStateChange";
		public static string QSBQuantumShuffle = "QSBQuantumShuffle";
		public static string QSBQuantumAuthority = "QSBQuantumAuthority";
		public static string QSBMoonStateChange = "QSBMoonStateChange";
		public static string QSBIdentifyFrequency = "QSBIdentifyFrequency";
		public static string QSBIdentifySignal = "QSBIdentifySignal";
		public static string QSBTextTranslated = "QSBTextTranslated";
		public static string QSBEnterShrine = "QSBEnterShrine";
		public static string QSBExitShrine = "QSBExitShrine";
		public static string QSBPlayerEntangle = "QSBPlayerEntangle";
		public static string QSBDropItem = "QSBDropItem";
		public static string QSBSocketItem = "QSBSocketItem";
		public static string QSBMoveToCarry = "QSBMoveToCarry";
		public static string QSBStartStatue = "QSBStartStatue";
		public static string QSBPlayerKick = "QSBPlayerKick";
		public static string QSBEnterPlatform = "QSBEnterPlatform";
		public static string QSBExitPlatform = "QSBExitPlatform";
		public static string QSBCampfireState = "QSBCampfireState";
		public static string QSBMarshmallowEvent = "QSBMarshmallowEvent";
	}
}