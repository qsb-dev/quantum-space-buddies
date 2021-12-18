using OWML.Common;
using QSB.Anglerfish.Events;
using QSB.Animation.NPC.Events;
using QSB.Animation.Player.Events;
using QSB.AuthoritySync;
using QSB.CampfireSync.Events;
using QSB.ClientServerStateSync.Events;
using QSB.ConversationSync.Events;
using QSB.DeathSync.Events;
using QSB.ElevatorSync.Events;
using QSB.GeyserSync.Events;
using QSB.ItemSync.Events;
using QSB.JellyfishSync.Events;
using QSB.LogSync.Events;
using QSB.MeteorSync.Events;
using QSB.OrbSync.Events;
using QSB.Player.Events;
using QSB.QuantumSync.Events;
using QSB.RespawnSync.Events;
using QSB.RoastingSync.Events;
using QSB.SatelliteSync.Events;
using QSB.SaveSync.Events;
using QSB.ShipSync.Events;
using QSB.ShipSync.Events.Component;
using QSB.ShipSync.Events.Hull;
using QSB.StatueSync.Events;
using QSB.TimeSync.Events;
using QSB.Tools.FlashlightTool.Events;
using QSB.Tools.ProbeLauncherTool.Events;
using QSB.Tools.ProbeTool.Events;
using QSB.Tools.SignalscopeTool.Events;
using QSB.Tools.SignalscopeTool.FrequencySync.Events;
using QSB.Tools.TranslatorTool.Events;
using QSB.Tools.TranslatorTool.TranslationSync.Events;
using QSB.TornadoSync.Events;
using QSB.Utility;
using QSB.Utility.Events;
using QSB.ZeroGCaveSync.Events;
using System.Collections.Generic;

namespace QSB.Events
{
	public static class QSBEventManager
	{
		public static bool Ready { get; private set; }

		private static List<IQSBEvent> _eventList = new();

		public static void Init()
		{
			BaseQSBEvent._msgType = 0;
			_eventList = new List<IQSBEvent>
			{
				// Player
				new PlayerReadyEvent(),
				new PlayerJoinEvent(),
				new PlayerSuitEvent(),
				new PlayerFlashlightEvent(),
				new PlayerSignalscopeEvent(),
				new PlayerTranslatorEvent(),
				new EquipProbeLauncherEvent(),
				new PlayerProbeEvent(),
				new PlayerDeathEvent(),
				new RequestStateResyncEvent(),
				new PlayerInformationEvent(),
				new ChangeAnimTypeEvent(),
				new ServerTimeEvent(),
				new PlayerEntangledEvent(),
				new PlayerKickEvent(),
				new EnterExitRoastingEvent(),
				new MarshmallowEventEvent(),
				new AnimationTriggerEvent(),
				new PlayerRespawnEvent(),
				new ProbeStartRetrieveEvent(),
				new RetrieveProbeEvent(),
				new LaunchProbeEvent(),
				new PlayerRetrieveProbeEvent(),
				new PlayerLaunchProbeEvent(),
				new EndLoopEvent(),
				new StartLoopEvent(),
				new ServerStateEvent(),
				new ClientStateEvent(),
				new DebugEvent(),
				new SatelliteProjectorEvent(),
				new SatelliteProjectorSnapshotEvent(),
				new LaunchCodesEvent(),
				new RequestGameStateEvent(),
				new GameStateEvent(),
				// World Objects
				new ElevatorEvent(),
				new GeyserEvent(),
				new OrbDragEvent(),
				new OrbSlotEvent(),
				new SocketStateChangeEvent(),
				new MultiStateChangeEvent(),
				new SetAsTranslatedEvent(),
				new QuantumShuffleEvent(),
				new MoonStateChangeEvent(),
				new EnterLeaveEvent(),
				new QuantumAuthorityEvent(),
				new DropItemEvent(),
				new SocketItemEvent(),
				new MoveToCarryEvent(),
				new StartStatueEvent(),
				new CampfireStateEvent(),
				new AnglerChangeStateEvent(),
				new MeteorPreLaunchEvent(),
				new MeteorLaunchEvent(),
				new MeteorSpecialImpactEvent(),
				new FragmentDamageEvent(),
				new FragmentResyncEvent(),
				new JellyfishRisingEvent(),
				new TornadoFormStateEvent(),
				// Conversation/dialogue/exploration
				new ConversationEvent(),
				new ConversationStartEndEvent(),
				new DialogueConditionEvent(),
				new RevealFactEvent(),
				new IdentifyFrequencyEvent(),
				new IdentifySignalEvent(),
				new NpcAnimationEvent(),
				new AuthorityQueueEvent(),
				// Ship
				new FlyShipEvent(),
				new HatchEvent(),
				new FunnelEnableEvent(),
				new HullImpactEvent(),
				new HullDamagedEvent(),
				new HullChangeIntegrityEvent(),
				new HullRepairedEvent(),
				new HullRepairTickEvent(),
				new ComponentDamagedEvent(),
				new ComponentRepairedEvent(),
				new ComponentRepairTickEvent(),
				new SatelliteNodeRepairTick(),
				new SatelliteNodeRepaired()
			};

			if (UnitTestDetector.IsInUnitTest)
			{
				return;
			}

			_eventList.ForEach(ev => ev.SetupListener());

			Ready = true;

			DebugLog.DebugWrite("Event Manager ready.", MessageType.Success);
		}

		public static void Reset()
		{
			Ready = false;
			_eventList.ForEach(ev => ev.CloseListener());
			_eventList = new List<IQSBEvent>();
		}

		public static void FireEvent(string eventName)
		{
			if (!QSBCore.IsInMultiplayer)
			{
				return;
			}

			GlobalMessenger.FireEvent(eventName);
		}

		public static void FireEvent<T>(string eventName, T arg)
		{
			if (!QSBCore.IsInMultiplayer)
			{
				DebugLog.ToConsole($"Warning - Tried to send event {eventName} while not connected to/hosting server.", MessageType.Warning);
				return;
			}

			GlobalMessenger<T>.FireEvent(eventName, arg);
		}

		public static void FireEvent<T, U>(string eventName, T arg1, U arg2)
		{
			if (!QSBCore.IsInMultiplayer)
			{
				DebugLog.ToConsole($"Warning - Tried to send event {eventName} while not connected to/hosting server.", MessageType.Warning);
				return;
			}

			GlobalMessenger<T, U>.FireEvent(eventName, arg1, arg2);
		}

		public static void FireEvent<T, U, V>(string eventName, T arg1, U arg2, V arg3)
		{
			if (!QSBCore.IsInMultiplayer)
			{
				DebugLog.ToConsole($"Warning - Tried to send event {eventName} while not connected to/hosting server.", MessageType.Warning);
				return;
			}

			GlobalMessenger<T, U, V>.FireEvent(eventName, arg1, arg2, arg3);
		}

		public static void FireEvent<T, U, V, W>(string eventName, T arg1, U arg2, V arg3, W arg4)
		{
			if (!QSBCore.IsInMultiplayer)
			{
				DebugLog.ToConsole($"Warning - Tried to send event {eventName} while not connected to/hosting server.", MessageType.Warning);
				return;
			}

			GlobalMessenger<T, U, V, W>.FireEvent(eventName, arg1, arg2, arg3, arg4);
		}

		public static void FireEvent<T, U, V, W, X>(string eventName, T arg1, U arg2, V arg3, W arg4, X arg5)
		{
			if (!QSBCore.IsInMultiplayer)
			{
				DebugLog.ToConsole($"Warning - Tried to send event {eventName} while not connected to/hosting server.", MessageType.Warning);
				return;
			}

			GlobalMessenger<T, U, V, W, X>.FireEvent(eventName, arg1, arg2, arg3, arg4, arg5);
		}

		public static void FireEvent<T, U, V, W, X, Y>(string eventName, T arg1, U arg2, V arg3, W arg4, X arg5, Y arg6)
		{
			if (!QSBCore.IsInMultiplayer)
			{
				DebugLog.ToConsole($"Warning - Tried to send event {eventName} while not connected to/hosting server.", MessageType.Warning);
				return;
			}

			GlobalMessenger<T, U, V, W, X, Y>.FireEvent(eventName, arg1, arg2, arg3, arg4, arg5, arg6);
		}

		/// used to force set ForId for every sent event
		public static uint ForIdOverride = uint.MaxValue;
	}
}
