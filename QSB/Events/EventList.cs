using QSB.Animation;
using QSB.DeathSync;
using QSB.ElevatorSync;
using QSB.GeyserSync;
using QSB.OrbSync;
using QSB.TimeSync;
using QSB.Tools;
using QSB.TransformSync;
using System.Collections.Generic;

namespace QSB.Events
{
    /// <summary>
    /// Creates instances of all of the events QSB uses.
    /// </summary>
    public static class EventList
    {
        public static bool Ready { get; private set; }

        private static List<IQSBEvent> _eventList = new List<IQSBEvent>();

        public static void Init()
        {
            _eventList = new List<IQSBEvent>
            {
                new PlayerReadyEvent(),
                new PlayerJoinEvent(),
                new PlayerSuitEvent(),
                new PlayerFlashlightEvent(),
                new PlayerSignalscopeEvent(),
                new PlayerTranslatorEvent(),
                new PlayerProbeLauncherEvent(),
                new PlayerProbeEvent(),
                new PlayerSectorEvent(),
                new PlayerLeaveEvent(),
                new PlayerDeathEvent(),
                new PlayerStatesRequestEvent(),
                new ElevatorEvent(),
                new GeyserEvent(),
                new ServerTimeEvent(),
                new AnimTriggerEvent(),
                new OrbSlotEvent(),
                new OrbUserEvent()
            };

            _eventList.ForEach(ev => ev.SetupListener());

            Ready = true;
        }

        public static void Reset()
        {
            Ready = false;

            _eventList.ForEach(ev => ev.CloseListener());

            _eventList = new List<IQSBEvent>();
        }
    }
}
