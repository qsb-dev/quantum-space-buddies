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

        List<string> _eventTypes = new List<string>
        {
            "TurnOnFlashlight",
            "TurnOffFlashlight",
            "SuitUp",
            "RemoveSuit",
            "EquipSignalscope",
            "UnequipSignalscope"
        };

        void Awake()
        {
            LocalInstance = this;
            _eventTypes.ForEach(x => GlobalMessenger.AddListener(x, () => EventHandler.LocalInstance.Send(x)));
        }
        
    }
}
