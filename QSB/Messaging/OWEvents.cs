namespace QSB.Messaging;

/// <summary>
/// global messenger events built into outer wilds
/// that are also used by qsb.
/// <para/>
/// don't change unless they change in-game!
/// </summary>
public static class OWEvents
{
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
	public const string PlayerEnterQuantumMoon = nameof(PlayerEnterQuantumMoon);
	public const string PlayerExitQuantumMoon = nameof(PlayerExitQuantumMoon);
	public const string EnterRoastingMode = nameof(EnterRoastingMode);
	public const string ExitRoastingMode = nameof(ExitRoastingMode);
	public const string EnterFlightConsole = nameof(EnterFlightConsole);
	public const string ExitFlightConsole = nameof(ExitFlightConsole);
	public const string EnterShip = nameof(EnterShip);
	public const string ExitShip = nameof(ExitShip);
	public const string EyeStateChanged = nameof(EyeStateChanged);
	public const string FlickerOffAndOn = nameof(FlickerOffAndOn);
	public const string EnterDreamWorld = nameof(EnterDreamWorld);
	public const string ExitDreamWorld = nameof(ExitDreamWorld);
	public const string EnterRemoteFlightConsole = nameof(EnterRemoteFlightConsole);
	public const string ExitRemoteFlightConsole = nameof(ExitRemoteFlightConsole);
	public const string StartShipIgnition = nameof(StartShipIgnition);
	public const string CompleteShipIgnition = nameof(CompleteShipIgnition);
	public const string CancelShipIgnition = nameof(CancelShipIgnition);
}