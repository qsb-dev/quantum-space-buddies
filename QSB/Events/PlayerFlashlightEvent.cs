using QSB.Messaging;

namespace QSB.Events
{
    class PlayerFlashlightEvent : QSBEvent
    {
        public override MessageType Type => MessageType.FlashlightActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener("TurnOnFlashlight", () => SendEvent(PlayerRegistry.LocalPlayer.NetId, true));
            GlobalMessenger.AddListener("TurnOffFlashlight", () => SendEvent(PlayerRegistry.LocalPlayer.NetId, false));
        }

        public override void OnReceive(uint sender, object[] data)
        {
            var player = PlayerRegistry.GetPlayer(sender);
            var tool = player.FlashLight;
            player.UpdateState(State.Flashlight, (bool)data[0]);
            if ((bool)data[0] == true)
            {
                tool.TurnOn();
            }
            else
            {
                tool.TurnOff();
            }
        }

        public override void OnReceiveLocal(object[] data)
        {
            var tool = PlayerRegistry.LocalPlayer.FlashLight;
            if ((bool)data[0] == true)
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
