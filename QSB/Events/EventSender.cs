using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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

            foreach (var item in EventList)
            {
                DebugLog.ToConsole($"Adding listener(s) for {item.Type}");
                item.SetupListener();
            }
        }

        public static void SendEvent(QSBEvent _event, uint sender, params object[] data)
        {
            var message = new EventMessage
            {
                SenderId = sender,
                EventType = (int)_event.Type,
                Data = data
            };
            EventHandler.LocalInstance.Send(message);
        }
    }
}
