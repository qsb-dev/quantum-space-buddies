﻿namespace QSB.Events
{
	public static class EventNames
	{
		// Built into Outer Wilds -- don't change unless they change in-game!
		public const string TurnOnFlashlight = nameof(TurnOnFlashlight);
		public const string TurnOffFlashlight = nameof(TurnOffFlashlight);
		public const string ProbeLauncherEquipped = nameof(ProbeLauncherEquipped);
		public const string ProbeLauncherUnequipped = nameof(ProbeLauncherUnequipped);
		public const string EquipSignalscope = nameof(EquipSignalscope);
		public const string UnequipSignalscope = nameof(UnequipSignalscope);
		public const string SuitUp = nameof(SuitUp);
		public const string RemoveSuit = nameof(RemoveSuit);
		public const string EquipTranslator = nameof(EquipTranslator);
		public const string UnequipTranslator = nameof(UnequipTranslator);
		public const string WakeUp = nameof(WakeUp);
		public const string DialogueConditionChanged = nameof(DialogueConditionChanged);
		public const string PlayerEnterQuantumMoon = nameof(PlayerEnterQuantumMoon);
		public const string PlayerExitQuantumMoon = nameof(PlayerExitQuantumMoon);
		public const string EnterRoastingMode = nameof(EnterRoastingMode);
		public const string ExitRoastingMode = nameof(ExitRoastingMode);
		public const string EnterFlightConsole = nameof(EnterFlightConsole);
		public const string ExitFlightConsole = nameof(ExitFlightConsole);
		public const string EnterShip = nameof(EnterShip);
		public const string ExitShip = nameof(ExitShip);

		// Custom event names -- change if you want! These can be anything, as long as both
		// sides of the GlobalMessenger (fireevent and addlistener) reference the same thing.
		public const string QSBPlayerDeath = nameof(QSBPlayerDeath);
		public const string QSBPlayerReady = nameof(QSBPlayerReady);
		public const string QSBServerTime = nameof(QSBServerTime);
		public const string QSBStartLift = nameof(QSBStartLift);
		public const string QSBOrbSlot = nameof(QSBOrbSlot);
		public const string QSBOrbUser = nameof(QSBOrbUser);
		public const string QSBConversation = nameof(QSBConversation);
		public const string QSBConversationStartEnd = nameof(QSBConversationStartEnd);
		public const string QSBChangeAnimType = nameof(QSBChangeAnimType);
		public const string QSBPlayerInformation = nameof(QSBPlayerInformation);
		public const string QSBRevealFact = nameof(QSBRevealFact);
		public const string QSBSocketStateChange = nameof(QSBSocketStateChange);
		public const string QSBMultiStateChange = nameof(QSBMultiStateChange);
		public const string QSBQuantumShuffle = nameof(QSBQuantumShuffle);
		public const string QSBQuantumAuthority = nameof(QSBQuantumAuthority);
		public const string QSBMoonStateChange = nameof(QSBMoonStateChange);
		public const string QSBIdentifyFrequency = nameof(QSBIdentifyFrequency);
		public const string QSBIdentifySignal = nameof(QSBIdentifySignal);
		public const string QSBTextTranslated = nameof(QSBTextTranslated);
		public const string QSBEnterShrine = nameof(QSBEnterShrine);
		public const string QSBExitShrine = nameof(QSBExitShrine);
		public const string QSBPlayerEntangle = nameof(QSBPlayerEntangle);
		public const string QSBDropItem = nameof(QSBDropItem);
		public const string QSBSocketItem = nameof(QSBSocketItem);
		public const string QSBMoveToCarry = nameof(QSBMoveToCarry);
		public const string QSBStartStatue = nameof(QSBStartStatue);
		public const string QSBPlayerKick = nameof(QSBPlayerKick);
		public const string QSBEnterPlatform = nameof(QSBEnterPlatform);
		public const string QSBExitPlatform = nameof(QSBExitPlatform);
		public const string QSBMarshmallowEvent = nameof(QSBMarshmallowEvent);
		public const string QSBAnimTrigger = nameof(QSBAnimTrigger);
		public const string QSBEnterNonNomaiHeadZone = nameof(QSBEnterNonNomaiHeadZone);
		public const string QSBExitNonNomaiHeadZone = nameof(QSBExitNonNomaiHeadZone);
		public const string QSBNpcAnimEvent = nameof(QSBNpcAnimEvent);
		public const string QSBHatchState = nameof(QSBHatchState);
		public const string QSBEnableFunnel = nameof(QSBEnableFunnel);
		public const string QSBHullImpact = nameof(QSBHullImpact);
		public const string QSBHullDamaged = nameof(QSBHullDamaged);
		public const string QSBHullChangeIntegrity = nameof(QSBHullChangeIntegrity);
		public const string QSBHullRepaired = nameof(QSBHullRepaired);
		public const string QSBHullRepairTick = nameof(QSBHullRepairTick);
		public const string QSBComponentDamaged = nameof(QSBComponentDamaged);
		public const string QSBComponentRepaired = nameof(QSBComponentRepaired);
		public const string QSBComponentRepairTick = nameof(QSBComponentRepairTick);
		public const string QSBPlayerRespawn = nameof(QSBPlayerRespawn);
		public const string QSBProbeEvent = nameof(QSBProbeEvent);
		public const string QSBProbeStartRetrieve = nameof(QSBProbeStartRetrieve);
		public const string QSBRetrieveProbe = nameof(QSBRetrieveProbe);
		public const string QSBPlayerRetrieveProbe = nameof(QSBPlayerRetrieveProbe);
		public const string QSBLaunchProbe = nameof(QSBLaunchProbe);
		public const string QSBPlayerLaunchProbe = nameof(QSBPlayerLaunchProbe);
		public const string QSBEndLoop = nameof(QSBEndLoop);
		public const string QSBStartLoop = nameof(QSBStartLoop);
		public const string QSBServerState = nameof(QSBServerState);
		public const string QSBClientState = nameof(QSBClientState);
		public const string QSBDebugEvent = nameof(QSBDebugEvent);
		public const string QSBEnterNomaiHeadZone = nameof(QSBEnterNomaiHeadZone);
		public const string QSBExitNomaiHeadZone = nameof(QSBExitNomaiHeadZone);
		public const string QSBEnterSatelliteCamera = nameof(QSBEnterSatelliteCamera);
		public const string QSBExitSatelliteCamera = nameof(QSBExitSatelliteCamera);
		public const string QSBSatelliteSnapshot = nameof(QSBSatelliteSnapshot);
		public const string QSBMeteorPreLaunch = nameof(QSBMeteorPreLaunch);
		public const string QSBMeteorLaunch = nameof(QSBMeteorLaunch);
		public const string QSBMeteorSpecialImpact = nameof(QSBMeteorSpecialImpact);
		public const string QSBFragmentDamage = nameof(QSBFragmentDamage);
		public const string QSBFragmentResync = nameof(QSBFragmentResync);
		public const string QSBLearnLaunchCodes = nameof(QSBLearnLaunchCodes);
		public const string QSBSatelliteRepairTick = nameof(QSBSatelliteRepairTick);
		public const string QSBSatelliteRepaired = nameof(QSBSatelliteRepairTick);
		public const string QSBAuthorityQueue = nameof(QSBAuthorityQueue);
		public const string QSBJellyfishRising = nameof(QSBJellyfishRising);
	}
}
