using QSB.Messaging;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSB.Events
{
    class PlayerReadyEvent : QSBEvent<ToggleMessage>
    {
        public override MessageType Type => MessageType.PlayerReady;

        public override void SetupListener()
        {
            GlobalMessenger<bool>.AddListener("QSBPlayerReady", ready => SendEvent(
                new ToggleMessage { 
                    SenderId = PlayerRegistry.LocalPlayer.NetId, 
                    ToggleValue = ready 
                }));
        }

        public override void OnReceive(ToggleMessage message)
        {
            return; 
        }

        public override void OnServerReceive(ToggleMessage message)
        {
            DebugLog.ToConsole("Received ready message from " + message.SenderId);
            PlayerRegistry.GetPlayer(message.SenderId).IsReady = message.ToggleValue;
            PlayerState.LocalInstance.Send();
        }
    }
}
