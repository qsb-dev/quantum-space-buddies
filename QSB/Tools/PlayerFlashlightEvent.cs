using QSB.Events;
using QSB.Messaging;

namespace QSB.Tools
{
    public class PlayerFlashlightEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.FlashlightActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener(EventNames.TurnOnFlashlight, () => SendEvent(CreateMessage(true)));
            GlobalMessenger.AddListener(EventNames.TurnOffFlashlight, () => SendEvent(CreateMessage(false)));
        }

        private ToggleMessage CreateMessage(bool value) => new ToggleMessage
        {
            SenderId = LocalPlayerId,
            ToggleValue = value
        };

        public override void OnReceiveRemote(ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            player.UpdateState(State.Flashlight, message.ToggleValue);
            if (!IsInUniverse)
            {
                return;
            }
            player.FlashLight?.UpdateState(message.ToggleValue);
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            PlayerRegistry.LocalPlayer.UpdateState(State.Flashlight, message.ToggleValue);
        }
    }
}
