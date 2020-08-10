namespace QSB.Events
{
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
            //new PlayerProbeEvent();
            //new PlayerSectorChange();
            new PlayerJoinEvent();
            new PlayerLeaveEvent();
            new PlayerDeathEvent();

            Ready = true;
        }
    }
}
