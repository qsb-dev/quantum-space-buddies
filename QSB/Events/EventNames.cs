namespace QSB.Events
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
		public const string EyeStateChanged = nameof(EyeStateChanged);

		// Custom event names -- change if you want! These can be anything, as long as both
		// sides of the GlobalMessenger (fireevent and addlistener) reference the same thing.
		public const string QSBDropItem = nameof(QSBDropItem);
		public const string QSBSocketItem = nameof(QSBSocketItem);
		public const string QSBMoveToCarry = nameof(QSBMoveToCarry);
		public const string QSBMarshmallowEvent = nameof(QSBMarshmallowEvent);
		public const string QSBAnimTrigger = nameof(QSBAnimTrigger);
		public const string QSBNpcAnimEvent = nameof(QSBNpcAnimEvent);
		public const string QSBProbeEvent = nameof(QSBProbeEvent);
		public const string QSBProbeStartRetrieve = nameof(QSBProbeStartRetrieve);
		public const string QSBRetrieveProbe = nameof(QSBRetrieveProbe);
		public const string QSBPlayerRetrieveProbe = nameof(QSBPlayerRetrieveProbe);
		public const string QSBLaunchProbe = nameof(QSBLaunchProbe);
		public const string QSBPlayerLaunchProbe = nameof(QSBPlayerLaunchProbe);
	}
}
