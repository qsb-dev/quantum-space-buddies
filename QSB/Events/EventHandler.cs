using System.Collections;
using System.Collections.Generic;
using OWML.ModHelper.Events;
using QSB.Animation;
using QSB.Messaging;
using QSB.TransformSync;
using UnityEngine;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class EventHandler : NetworkBehaviour
    {
        public static EventHandler LocalInstance;

        private MessageHandler<EventMessage> _eventHandler;

        private void Awake()
        {
            LocalInstance = this;

            _eventHandler = new MessageHandler<EventMessage>();
            _eventHandler.OnClientReceiveMessage += OnClientReceiveMessage;
            _eventHandler.OnServerReceiveMessage += OnServerReceiveMessage;
        }

        public void Send(string eventType)
        {
            StartCoroutine(SendEvent(eventType));
        }

        private IEnumerator SendEvent(string eventType)
        {
            yield return new WaitUntil(() => PlayerTransformSync.LocalInstance != null);
            var message = new EventMessage
            {
                EventType = eventType,
                SenderId = PlayerTransformSync.LocalInstance.netId.Value
            };
            _eventHandler.SendToServer(message);
        }

        private void OnServerReceiveMessage(EventMessage message)
        {
            _eventHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(EventMessage message)
        {
            if (message.SenderId != PlayerTransformSync.LocalInstance.netId.Value)
            {
                DebugLog.ToScreen("Received event message!");
                switch (message.EventType)
                {
                    case "TurnOnFlashlight":
                        Finder.GetPlayerFlashlight(message.SenderId).TurnOn();
                        Finder.UpdateState(message.SenderId, State.Flashlight, true);
                        break;
                    case "TurnOffFlashlight":
                        Finder.GetPlayerFlashlight(message.SenderId).TurnOff();
                        Finder.UpdateState(message.SenderId, State.Flashlight, false);
                        break;
                    case "SuitUp":
                        Finder.UpdateState(message.SenderId, State.Suit, true);
                        break;
                    case "RemoveSuit":
                        Finder.UpdateState(message.SenderId, State.Suit, false);
                        break;
                    case "EquipSignalscope":
                        DebugLog.ToScreen("Equip signalscope");
                        break;
                }
            }
        }
    }
}
