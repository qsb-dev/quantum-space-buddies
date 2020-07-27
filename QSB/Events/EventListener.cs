using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.Events
{
    class EventListener : MonoBehaviour
    {
        public static EventListener LocalInstance;

        void Awake()
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
