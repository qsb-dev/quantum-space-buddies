namespace QSB.Events
{
    public static class EventList
    {
        public static void Init()
        {
            new PlayerFlashlightEvent();
            new PlayerSignalscopeEvent();
            new PlayerTranslatorEvent();
            new PlayerProbeLauncherEvent();
            //new PlayerProbeEvent();
            //new PlayerSectorChange();
            new PlayerJoinEvent();
            //new PlayerLeaveEvent();
        }
    }
}
