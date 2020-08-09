using QSB.Messaging;

namespace QSB.Events
{
    class PlayerFlashlightEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.FlashlightActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener("TurnOnFlashlight", () => SendEvent(new ToggleMessage { On = true }));
            GlobalMessenger.AddListener("TurnOffFlashlight", () => SendEvent(new ToggleMessage { On = false }));
        }

        public override void OnReceive(uint sender, ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(sender);
            var tool = player.FlashLight;
            player.UpdateState(State.Flashlight, message.On);
            if (message.On)
            {
                tool.TurnOn();
            }
            else
            {
                tool.TurnOff();
            }
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            var tool = PlayerRegistry.LocalPlayer.FlashLight;
            if (message.On)
            {
                tool.TurnOn();
            }
            else
            {
                tool.TurnOff();
            }
        }
    }
}
