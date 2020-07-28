using System;
using UnityEngine;

namespace QSB.Events
{
    public class EventListener : MonoBehaviour
    {
        public static EventListener LocalInstance;

        private void Awake()
        {
            LocalInstance = this;
            foreach (var item in Enum.GetNames(typeof(EventType)))
            {
                GlobalMessenger.AddListener(item, () => SendEvent(item));
            }
        }
        
        private void SendEvent(string eventName)
        {
            EventHandler.LocalInstance.Send((EventType)Enum.Parse(typeof(EventType), eventName));
        }
    }
}
