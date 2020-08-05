using System.Collections;
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
            // this code is t r a s h.
            // sorry.
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            switch ((EventType)message.EventType)
            {
                case EventType.TurnOnFlashlight:
                    player.UpdateState(State.Flashlight, true);
                    break;
                case EventType.TurnOffFlashlight:
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
                    break;
                case EventType.UnequipSignalscope:
                    player.UpdateState(State.Signalscope, false);
                    break;
                case EventType.EquipTranslator:
                    player.UpdateState(State.Translator, true);
                    break;
                case EventType.UnequipTranslator:
                    player.UpdateState(State.Translator, false);
                    break;
                case EventType.ProbeLauncherEquipped:
                    player.UpdateState(State.ProbeLauncher, true);
                    break;
                case EventType.ProbeLauncherUnequipped:
                    player.UpdateState(State.ProbeLauncher, false);
                    break;
                case EventType.RetrieveProbe:
                    player.UpdateState(State.ProbeActive, false);
                    break;
                case EventType.LaunchProbe:
                    player.UpdateState(State.ProbeActive, true);
                    break;
            }
            if (message.SenderId == PlayerRegistry.LocalPlayer.NetId)
            {
                return;
            }
            switch ((EventType)message.EventType)
            {
                case EventType.TurnOnFlashlight:
                    player.FlashLight.TurnOn();
                    break;
                case EventType.TurnOffFlashlight:
                    player.FlashLight.TurnOff();
                    break;
                case EventType.EquipSignalscope:
                    player.Signalscope.EquipTool();
                    break;
                case EventType.UnequipSignalscope:
                    player.Signalscope.UnequipTool();
                    break;
                case EventType.EquipTranslator:
                    player.Translator.EquipTool();
                    break;
                case EventType.UnequipTranslator:
                    player.Translator.UnequipTool();
                    break;
                case EventType.ProbeLauncherEquipped:
                    player.ProbeLauncher.EquipTool();
                    break;
                case EventType.ProbeLauncherUnequipped:
                    player.ProbeLauncher.UnequipTool();
                    break;
                case EventType.RetrieveProbe:
                    player.Probe.Deactivate();
                    break;
                case EventType.LaunchProbe:
                    player.Probe.Activate();
                    break;
            }
        }
    }
}
