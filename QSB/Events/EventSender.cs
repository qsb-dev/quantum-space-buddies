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

            foreach (var item in EventList)
            {
                DebugLog.ToConsole($"Adding listener(s) for {item.Type}");
                item.SetupListener();
            }
        }

        public static void SendEvent(QSBEvent _event, params object[] data)
        {
            DebugLog.ToConsole($"Sending {_event.Type}");
            var message = new EventMessage
            {
                SenderId = PlayerRegistry.LocalPlayer.NetId,
                EventType = (int)_event.Type,
                Data = data
            };
            EventHandler.LocalInstance.Send(message);
        }
    }
}
