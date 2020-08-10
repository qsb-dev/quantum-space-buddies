using QSB.Messaging;
using QSB.Utility;

namespace QSB.Events
{
    public class PlayerDeathEvent : QSBEvent<PlayerDeathMessage>
    {
        public override MessageType Type => MessageType.PlayerDeath;

        public override void SetupListener()
        {
            GlobalMessenger<DeathType>.AddListener(EventNames.QSBPlayerDeath, type => SendEvent(CreateMessage(type)));
        }

        private PlayerDeathMessage CreateMessage(DeathType type) => new PlayerDeathMessage
        {
            SenderId = LocalPlayerId,
            DeathType = type
        };

        public override void OnReceiveRemote(PlayerDeathMessage message)
        {
            var playerName = PlayerRegistry.GetPlayer(message.SenderId).Name;
            var deathMessage = Necronomicon.GetPhrase(message.DeathType);
            DebugLog.ToAll(string.Format(deathMessage, playerName));
        }
    }
}
