using QSB.Events;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.DeathSync
{
    public class PlayerDeathEvent : QSBEvent<PlayerDeathMessage>
    {
        public override MessageType Type => MessageType.PlayerDeath;

        public override void SetupListener()
        {
            GlobalMessenger<DeathType>.AddListener(EventNames.QSBPlayerDeath, Handler);
        }

        public override void CloseListener()
        {
            GlobalMessenger<DeathType>.RemoveListener(EventNames.QSBPlayerDeath, Handler);
        }

        private void Handler(DeathType type) => SendEvent(CreateMessage(type));

        private PlayerDeathMessage CreateMessage(DeathType type) => new PlayerDeathMessage
        {
            FromId = LocalPlayerId,
            AboutId = LocalPlayerId,
            DeathType = type
        };

        public override void OnReceiveRemote(PlayerDeathMessage message)
        {
            var playerName = PlayerRegistry.GetPlayer(message.AboutId).Name;
            var deathMessage = Necronomicon.GetPhrase(message.DeathType);
            DebugLog.ToAll(string.Format(deathMessage, playerName));
        }

        public override void OnReceiveLocal(PlayerDeathMessage message) => OnReceiveRemote(message);
    }
}
