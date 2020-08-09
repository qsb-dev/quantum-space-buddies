using System.Collections.Generic;

namespace QSB.Events
{
    public static class EventSender
    {
        public static List<QSBEvent> EventList = new List<QSBEvent>();

        public static void Init()
        {
            EventList.Add(new PlayerFlashlightEvent());
            EventList.Add(new PlayerSignalscopeEvent());
            EventList.Add(new PlayerTrasnlatorEvent());
            EventList.Add(new PlayerProbeLauncherEvent());
            //EventList.Add(new PlayerProbeEvent());
            //EventList.Add(new PlayerSectorChange());
            EventList.Add(new PlayerJoinEvent());
            //EventList.Add(new PlayerLeaveEvent());
        }
    }
}
