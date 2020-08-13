using QSB.ElevatorSync;
using QSB.GeyserSync;

namespace QSB.Events
{
    /// <summary>
    /// Creates instances of all of the events QSB uses.
    /// </summary>
    public static class EventList
    {
        public static bool Ready { get; private set; }

        public static void Init()
        {
            new PlayerReadyEvent();
            new PlayerSuitEvent();
            new PlayerFlashlightEvent();
            new PlayerSignalscopeEvent();
            new PlayerTranslatorEvent();
            new PlayerProbeLauncherEvent();
            new PlayerProbeEvent();
            new PlayerSectorEvent();
            new PlayerJoinEvent();
            new PlayerLeaveEvent();
            new PlayerDeathEvent();
            new PlayerStatesRequestEvent();
            new ElevatorEvent();
            new GeyserEvent();

            Ready = true;
        }
    }
}
