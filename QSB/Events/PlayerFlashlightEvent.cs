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
                    SenderId = PlayerRegistry.LocalPlayer.NetId,
                    ToggleValue = true
                }));
            GlobalMessenger.AddListener("TurnOffFlashlight", () => SendEvent(
                new ToggleMessage {
                    SenderId = PlayerRegistry.LocalPlayer.NetId,
                    ToggleValue = false
                }));
        }

        public override void OnReceive(ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            player.UpdateState(State.Flashlight, message.ToggleValue);
            player.FlashLight.UpdateState(message.ToggleValue);
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            PlayerRegistry.LocalPlayer.FlashLight.UpdateState(message.ToggleValue);
        }
    }
}
