using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.Events
{
    class PlayerDeathEvent : QSBEvent<PlayerDeathMessage>
    {
        public override MessageType Type => MessageType.PlayerDeath;

        public override void SetupListener()
        {
            GlobalMessenger<DeathType>.AddListener("QSBPlayerDeath", type => SendEvent(new PlayerDeathMessage { SenderId = PlayerRegistry.LocalPlayer.NetId, DeathType = type }));
        }

        public override void OnReceive(PlayerDeathMessage message)
        {
            var playerName = PlayerRegistry.GetPlayer(message.SenderId).Name;
            var deathMessage = Necronomicon.GetPhrase(message.DeathType);
            DebugLog.ToAll(string.Format(deathMessage, playerName));
        }

        public override void OnReceiveLocal(PlayerDeathMessage message)
        {
            OnReceive(message);
        }
    }
}
