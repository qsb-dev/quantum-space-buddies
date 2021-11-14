namespace QSB.Events
{
	public enum EventType
	{
		/*
		 * MISC.
		 */
		DebugEvent,
		SatelliteProjector,
		SatelliteProjectorSnapshot,

		/*
		 * SERVER EVENTS
		 */

		ServerTime,
		StartStatue,
		EndLoop,
		StartLoop,
		ServerState,
		ClientState,

		/*
		 * PLAYER EVENTS
		 */

		PlayerInformation,
		RequestStateResync,
		PlayerJoin,
		PlayerDeath,
		PlayerReady,
		PlayerKick,
		PlayerRespawn,
		EnterLeave,

		/*
		 * DIALOGUE
		 */

		Conversation,
		ConversationStartEnd,
		DialogueCondition,
		RevealFact,

		/*
		 * ANIMATION
		 */

		PlayInstrument,
		AnimTrigger,
		NpcAnimEvent,
		SuitActiveChange,

		/*
		 * ORBS
		 */

		OrbSlot,
		OrbUser,

		/*
		 * CAMPFIRES
		 */

		CampfireState,
		Roasting,
		MarshmallowEvent,

		/*
		 * WORLD OBJECTS
		 */

		Geyser,
		Elevator,

		/*
		 * ITEMS
		 */

		DropItem,
		SocketItem,
		MoveToCarry,

		/*
		 * QUANTUM OBJECTS
		 */

		SocketStateChange,
		MultiStateChange,
		QuantumShuffle,
		QuantumAuthority,
		MoonStateChange,
		PlayerEntangle,

		/*
		 * SHIP
		 */

		ComponentDamaged,
		ComponentRepaired,
		ComponentRepairTick,
		HullImpact,
		HullDamaged,
		HullChangeIntegrity,
		HullRepaired,
		HullRepairTick,
		FlyShip,
		OpenHatch,
		EnableFunnel,

		/*
		 * TOOLS
		 */

		// Flashlight
		FlashlightActiveChange,

		// Translator
		TranslatorActiveChange,
		TextTranslated,

		// Signalscope
		SignalscopeActiveChange,
		IdentifyFrequency,
		IdentifySignal,

		// Probe
		ProbeStartRetrieve,
		ProbeEvent,

		// Probe Launcher
		ProbeLauncherActiveChange,
		RetrieveProbe,
		PlayerRetrieveProbe,
		LaunchProbe,
		PlayerLaunchProbe,

		AnglerChangeState,

		MeteorPreLaunch,
		MeteorLaunch,
		FragmentDamage,
		FragmentResync
	}
}
