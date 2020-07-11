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
        }

        void Start()
        {
            GlobalMessenger.AddListener("TurnOnFlashlight", new Callback(this.FlashlightOn));
            GlobalMessenger.AddListener("TurnOffFlashlight", new Callback(this.FlashlightOff));
        }

        private void FlashlightOn()
        {
            DebugLog.ToAll("TurnOnFlashlight");
            EventHandler.LocalInstance.Send("TurnOnFlashlight");
        }

        private void FlashlightOff()
        {
            DebugLog.ToAll("TurnOffFlashlight");
            EventHandler.LocalInstance.Send("TurnOffFlashlight");
        }
    }
}
