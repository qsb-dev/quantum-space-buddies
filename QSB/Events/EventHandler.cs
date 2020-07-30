using System.Collections;
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

        public void Send(EventType eventType)
        {
            StartCoroutine(SendEvent(eventType));
        }

        private IEnumerator SendEvent(EventType eventType)
        {
            yield return new WaitUntil(() => PlayerTransformSync.LocalInstance != null);
            var message = new EventMessage
            {
                EventType = (int)eventType,
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
            if (message.SenderId == PlayerRegistry.LocalPlayer.NetId)
            {
                return;
            }
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            switch ((EventType)message.EventType)
            {
                case EventType.TurnOnFlashlight:
                    player.FlashLight.TurnOn();
                    player.UpdateState(State.Flashlight, true);
                    break;
                case EventType.TurnOffFlashlight:
                    player.FlashLight.TurnOn();
                    player.UpdateState(State.Flashlight, false);
                    break;
                case EventType.SuitUp:
                    player.UpdateState(State.Suit, true);
                    break;
                case EventType.RemoveSuit:
                    player.UpdateState(State.Suit, false);
                    break;
                case EventType.EquipSignalscope:
                    player.UpdateState(State.Signalscope, true);
                    player.Signalscope.EquipTool();
                    break;
                case EventType.UnequipSignalscope:
                    player.UpdateState(State.Signalscope, false);
                    player.Signalscope.UnequipTool();
                    break;
                case EventType.EquipTranslator:
                    player.UpdateState(State.Translator, true);
                    player.Translator.EquipTool();
                    break;
                case EventType.UnequipTranslator:
                    player.UpdateState(State.Translator, false);
                    player.Translator.UnequipTool();
                    break;
            }
        }
    }
}
