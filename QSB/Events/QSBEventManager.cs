using OWML.Common;
using QSB.Animation.NPC.Events;
using QSB.Animation.Player.Events;
using QSB.CampfireSync.Events;
using QSB.ClientServerStateSync.Events;
using QSB.ConversationSync.Events;
using QSB.DeathSync.Events;
using QSB.ElevatorSync.Events;
using QSB.FrequencySync.Events;
using QSB.GeyserSync.Events;
using QSB.ItemSync.Events;
using QSB.LogSync.Events;
using QSB.OrbSync.Events;
using QSB.Player.Events;
using QSB.ProbeSync.Events;
using QSB.QuantumSync.Events;
using QSB.RoastingSync.Events;
using QSB.SatelliteSync.Events;
using QSB.ShipSync.Events;
using QSB.ShipSync.Events.Component;
using QSB.ShipSync.Events.Hull;
using QSB.StatueSync.Events;
using QSB.TimeSync.Events;
using QSB.Tools.Events;
using QSB.Tools.ProbeLauncherTool.Events;
using QSB.TranslationSync.Events;
using QSB.Utility;
using QSB.Utility.Events;
using System.Collections.Generic;

namespace QSB.Events
{
	public static class QSBEventManager
	{
		public static bool Ready { get; private set; }

		private static List<IQSBEvent> _eventList = new List<IQSBEvent>();

		public static void Init()
		{
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
				// World Objects
				new ElevatorEvent(),
				new GeyserEvent(),
				new OrbSlotEvent(),
				new OrbUserEvent(),
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
				// Conversation/dialogue/exploration
				new ConversationEvent(),
				new ConversationStartEndEvent(),
				new DialogueConditionEvent(),
				new RevealFactEvent(),
				new IdentifyFrequencyEvent(),
				new IdentifySignalEvent(),
				new NpcAnimationEvent(),
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
				new ComponentRepairTickEvent()
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
	}
}