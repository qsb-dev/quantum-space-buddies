using System;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.Events
{
    public class EventListener : MonoBehaviour
    {
        public static EventListener LocalInstance;

        public List<EventType> ExclusionList = new List<EventType>
        {
            EventType.EquipSignalscope
        };

        private void Awake()
        {
            LocalInstance = this;
            foreach (var item in Enum.GetNames(typeof(EventType)))
            {
                if (!ExclusionList.Contains((EventType)Enum.Parse(typeof(EventType), item)))
                {
                    GlobalMessenger.AddListener(item, () => SendEvent(item));
                }
            }
            EquipSignalscope();

        }
        
        private void SendEvent(string eventName)
        {
            EventHandler.LocalInstance.Send((EventType)Enum.Parse(typeof(EventType), eventName));
        }

        private void EquipSignalscope()
        {
            GlobalMessenger<Signalscope>.AddListener("EquipSignalscope", (Signalscope scope) => SendEvent("EquipSignalscope"));
        }
    }
}
