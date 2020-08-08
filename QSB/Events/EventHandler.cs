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
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            if (message.SenderId == PlayerRegistry.LocalPlayer.NetId)
            {
                return;
            }
            switch ((EventType)message.EventType)
            {
                case EventType.TurnOnFlashlight:
                    player.UpdateState(State.Flashlight, true);
                    player.FlashLight.TurnOn();
                    break;
                case EventType.TurnOffFlashlight:
                    player.UpdateState(State.Flashlight, false);
                    player.FlashLight.TurnOff();
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
                case EventType.ProbeLauncherEquipped:
                    player.UpdateState(State.ProbeLauncher, true);
                    player.ProbeLauncher.EquipTool();
                    break;
                case EventType.ProbeLauncherUnequipped:
                    player.UpdateState(State.ProbeLauncher, false);
                    player.ProbeLauncher.UnequipTool();
                    break;
                case EventType.RetrieveProbe:
                    player.UpdateState(State.ProbeActive, false);
                    player.Probe.Deactivate();
                    break;
                case EventType.LaunchProbe:
                    player.UpdateState(State.ProbeActive, true);
                    player.Probe.Activate();
                    break;
            }
        }
    }
}
