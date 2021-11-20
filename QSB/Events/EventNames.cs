namespace QSB.Events
{
	public static class EventNames
	{
		// Built into Outer Wilds -- don't change unless they change in-game!
		public static string TurnOnFlashlight = "TurnOnFlashlight";
		public static string TurnOffFlashlight = "TurnOffFlashlight";
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
		public static string EnterShip = "EnterShip";
		public static string ExitShip = "ExitShip";
		public static string LaunchCodes = "LearnLaunchCodes";

		// Custom event names -- change if you want! These can be anything, as long as both
		// sides of the GlobalMessenger (fireevent and addlistener) reference the same thing.
		public static string QSBPlayerDeath = "QSBPlayerDeath";
		public static string QSBPlayerJoin = "QSBPlayerJoin";
		public static string QSBPlayerReady = "QSBPlayerReady";
		public static string QSBRequestStateResync = "QSBPlayerStatesRequest";
		public static string QSBServerTime = "QSBServerTime";
		public static string QSBStartLift = "QSBStartLift";
		public static string QSBGeyserState = "QSBGeyserState";
		public static string QSBOrbSlot = "QSBOrbSlot";
		public static string QSBOrbUser = "QSBOrbUser";
		public static string QSBConversation = "QSBConversation";
		public static string QSBConversationStartEnd = "QSBConversationStartEnd";
		public static string QSBChangeAnimType = "QSBPlayInstrument";
		public static string QSBPlayerInformation = "QSBServerSendPlayerStates";
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
		public static string QSBAnimTrigger = "QSBAnimTrigger";
		public static string QSBEnterNonNomaiHeadZone = "QSBEnterNonNomaiHeadZone";
		public static string QSBExitNonNomaiHeadZone = "QSBExitNonNomaiHeadZone";
		public static string QSBNpcAnimEvent = "QSBNpcAnimEvent";
		public static string QSBHatchState = "QSBHatchState";
		public static string QSBEnableFunnel = "QSBEnableFunnel";
		public static string QSBHullImpact = "QSBHullImpact";
		public static string QSBHullDamaged = "QSBHullDamaged";
		public static string QSBHullChangeIntegrity = "QSBHullChangeIntegrity";
		public static string QSBHullRepaired = "QSBHullRepaired";
		public static string QSBHullRepairTick = "QSBHullRepairTick";
		public static string QSBComponentDamaged = "QSBComponentDamaged";
		public static string QSBComponentRepaired = "QSBComponentRepaired";
		public static string QSBComponentRepairTick = "QSBComponentRepairTick";
		public static string QSBPlayerRespawn = "QSBPlayerRespawn";
		public static string QSBProbeEvent = "QSBProbeEvent";
		public static string QSBProbeStartRetrieve = "QSBProbeStartRetrieve";
		public static string QSBRetrieveProbe = "QSBRetrieveProbe";
		public static string QSBPlayerRetrieveProbe = "QSBPlayerRetrieveProbe";
		public static string QSBLaunchProbe = "QSBLaunchProbe";
		public static string QSBPlayerLaunchProbe = "QSBPlayerLaunchProbe";
		public static string QSBEndLoop = "QSBEndLoop";
		public static string QSBStartLoop = "QSBStartLoop";
		public static string QSBServerState = "QSBServerState";
		public static string QSBClientState = "QSBClientState";
		public static string QSBDebugEvent = "QSBDebugEvent";
		public static string QSBEnterNomaiHeadZone = "QSBEnterNomaiHeadZone";
		public static string QSBExitNomaiHeadZone = "QSBExitNomaiHeadZone";
		public static string QSBEnterSatelliteCamera = "QSBEnterSatelliteCamera";
		public static string QSBExitSatelliteCamera = "QSBExitSatelliteCamera";
		public static string QSBSatelliteSnapshot = "QSBSatelliteSnapshot";
		public static string QSBAnglerChangeState = "QSBAnglerChangeState";
		public static string QSBMeteorPreLaunch = "QSBMeteorPreLaunch";
		public static string QSBMeteorLaunch = "QSBMeteorLaunch";
		public static string QSBMeteorSpecialImpact = "QSBMeteorSpecialImpact";
		public static string QSBFragmentDamage = "QSBFragmentDamage";
		public static string QSBFragmentResync = "QSBFragmentResync";
	}
}
