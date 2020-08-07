using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.Events
{
    class PlayerFlashlightEvent : QSBEvent
    {
        public override EventType Type => EventType.FlashlightActiveChange;

        public override void OnReceive(uint sender, object[] data)
        {
            var player = PlayerRegistry.GetPlayer(sender);

            player.UpdateState(State.Flashlight, (bool)data[0]);
            if ((bool)data[0] == true)
            {
                player.FlashLight.TurnOn();
            }
            else
            {
                player.FlashLight.TurnOff();
            }
        }

        public override void OnReceiveLocal(object[] data)
        {
            if ((bool)data[0] == true)
            {
                PlayerRegistry.LocalPlayer.FlashLight.TurnOn();
            }
            else
            {
                PlayerRegistry.LocalPlayer.FlashLight.TurnOff();
            }
        }

        public override void SetupListener()
        {
            GlobalMessenger.AddListener("TurnOnFlashlight", () => EventSender.SendEvent(this, true));
            GlobalMessenger.AddListener("TurnOffFlashlight", () => EventSender.SendEvent(this, true));
        }
    }
}
