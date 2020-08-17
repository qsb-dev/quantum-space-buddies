using QSB.Events;
using QSB.Messaging;

namespace QSB.Tools
{
    public class PlayerFlashlightEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.FlashlightActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener(EventNames.TurnOnFlashlight, HandleTurnOn);
            GlobalMessenger.AddListener(EventNames.TurnOffFlashlight, HandleTurnOff);
        }

        public override void CloseListener()
        {
            GlobalMessenger.RemoveListener(EventNames.TurnOnFlashlight, HandleTurnOn);
            GlobalMessenger.RemoveListener(EventNames.TurnOffFlashlight, HandleTurnOff);
        }

        private void HandleTurnOn() => SendEvent(CreateMessage(true));
        private void HandleTurnOff() => SendEvent(CreateMessage(false));

        private ToggleMessage CreateMessage(bool value) => new ToggleMessage
        {
            SenderId = LocalPlayerId,
            ToggleValue = value
        };

        public override void OnReceiveRemote(ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            player?.UpdateState(State.Flashlight, message.ToggleValue);
            if (!QSBSceneManager.IsInUniverse)
            {
                return;
            }
            player?.FlashLight?.UpdateState(message.ToggleValue);
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            PlayerRegistry.LocalPlayer.UpdateState(State.Flashlight, message.ToggleValue);
        }
    }
}
