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
            switch (message.EventType)
            {
                case "TurnOnFlashlight":
                    FindPlayerObject(message.SenderId).GetComponentInChildren<QSBFlashlight>().TurnOn();
                    break;
                case "TurnOffFlashlight":
                    FindPlayerObject(message.SenderId).GetComponentInChildren<QSBFlashlight>().TurnOff();
                    break;
            }
        }

        private GameObject FindPlayerObject(uint id)
        {
            var markers = GameObject.FindObjectsOfType<PlayerHUDMarker>();
            foreach (var item in markers)
            {
                if (item.GetValue<uint>("_netId") == id)
                {
                    return item.gameObject;
                }
            }
            return null;
        }
    }
}
