using System.Collections;
using System.Linq;
using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;
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

        public void Send(EventMessage message)
        {
            StartCoroutine(SendEvent(message));
        }

        private IEnumerator SendEvent(EventMessage message)
        {
            yield return new WaitUntil(() => PlayerTransformSync.LocalInstance != null);
            _eventHandler.SendToServer(message);
        }

        private void OnServerReceiveMessage(EventMessage message)
        {
            _eventHandler.SendToAll(message);
        }

        private void OnClientReceiveMessage(EventMessage message)
        {
            if (message.SenderId == PlayerRegistry.LocalPlayer?.NetId)
            {
                return;
            }

            var _event = EventSender.EventList.First(x => x.Type == (EventType)message.EventType);
            _event.OnReceive(message.SenderId, message.Data);
        }
    }
}
