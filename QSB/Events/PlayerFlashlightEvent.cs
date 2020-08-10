using QSB.Messaging;

namespace QSB.Events
{
    class PlayerFlashlightEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.FlashlightActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener("TurnOnFlashlight", () => SendEvent(
                new ToggleMessage {
                    SenderId = LocalPlayerId,
                    ToggleValue = true
                }));
            GlobalMessenger.AddListener("TurnOffFlashlight", () => SendEvent(
                new ToggleMessage {
                    SenderId = LocalPlayerId,
                    ToggleValue = false
                }));
        }

        public override void OnReceiveRemote(ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            player.UpdateState(State.Flashlight, message.ToggleValue);
            player.FlashLight?.UpdateState(message.ToggleValue);
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            PlayerRegistry.LocalPlayer.UpdateState(State.Flashlight, message.ToggleValue);
        }
    }
}
