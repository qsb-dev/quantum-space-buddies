using OWML.ModHelper.Events;
using QSB.Animation;
using QSB.DeathSync;
using QSB.ElevatorSync;
using QSB.GeyserSync;
using QSB.TimeSync;
using QSB.Tools;
using QSB.TransformSync;
using QSB.Utility;
using System.Collections.Generic;

namespace QSB.Events
{
    /// <summary>
    /// Creates instances of all of the events QSB uses.
    /// </summary>
    public static class EventList
    {
        public static bool Ready { get; private set; }

        private static List<object> _eventList = new List<object>();

        public static void Init()
        {
            _eventList = new List<object>
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
                new ServerTimeEvent()
            };

            Ready = true;
        }

        public static void Reset()
        {
            Ready = false;
            foreach (var item in _eventList)
            {
                item.Invoke("CloseListener");
            }
            _eventList = new List<object>();
        }
    }
}
