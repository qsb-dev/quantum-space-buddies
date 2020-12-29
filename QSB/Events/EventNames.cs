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
		public static string ExitShip = "ExitShip";
		public static string RestartTimeLoop = "RestartTimeLoop";
		public static string WakeUp = "WakeUp";
		public static string DialogueCondition = "DialogueConditionChanged";

		// Custom event names -- change if you want! These can be anything, as long as both
		// sides of the GlobalMessenger (fireevent and setuplistener) reference the same thing.
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
		public static string QSBIdentifyFrequency = "QSBIdentifyFrequency";
		public static string QSBIdentifySignal = "QSBIdentifySignal";
		public static string QSBTextTranslated = "QSBTextTranslated";
	}
}