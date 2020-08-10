using QSB.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.Events
{
    class PlayerProbeEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.ProbeActiveChange;

        public override void SetupListener()
        {
            GlobalMessenger<SurveyorProbe>.AddListener("LaunchProbe", probe => SendEvent(
                new ToggleMessage {
                    SenderId = LocalPlayerId, 
                    ToggleValue = true 
                }));
            GlobalMessenger<SurveyorProbe>.AddListener("RetrieveProbe", probe => SendEvent(
                new ToggleMessage
                {
                    SenderId = LocalPlayerId,
                    ToggleValue = false
                }));
        }

        public override void OnReceiveRemote(ToggleMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            player.UpdateState(State.ProbeActive, message.ToggleValue);
            if (message.ToggleValue)
            {
                player.Probe.Activate();
            }
            else
            {
                player.Probe.Deactivate();
            }
        }

        public override void OnReceiveLocal(ToggleMessage message)
        {
            if (message.ToggleValue)
            {
                PlayerRegistry.LocalPlayer.Probe.Activate();
            }
            else
            {
                PlayerRegistry.LocalPlayer.Probe.Deactivate();
            }
        }
    }
}
