using QSB.Messaging;
using QSB.Utility;

namespace QSB.Events
{
    public class PlayerJoinEvent : QSBEvent<PlayerJoinMessage>
    {
        public override MessageType Type => MessageType.PlayerJoin;

        public override void SetupListener()
        {
            GlobalMessenger<string>.AddListener(EventNames.QSBPlayerJoin, Handler);
        }

        public override void CloseListener()
        {
            GlobalMessenger<string>.RemoveListener(EventNames.QSBPlayerJoin, Handler);
        }

        private void Handler(string name) => SendEvent(CreateMessage(name));

        private PlayerJoinMessage CreateMessage(string name) => new PlayerJoinMessage
        {
            AboutId = LocalPlayerId,
            PlayerName = name
        };

        public override void OnReceiveRemote(PlayerJoinMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.AboutId);
            player.Name = message.PlayerName;
            var text = $"{player.Name} joined!";
            DebugLog.ToAll(text, OWML.Common.MessageType.Info);
        }

        public override void OnReceiveLocal(PlayerJoinMessage message)
        {
            var player = PlayerRegistry.GetPlayer(PlayerRegistry.LocalPlayerId);
            player.Name = message.PlayerName;
            var text = $"Connected to server as {player.Name}.";
            DebugLog.ToAll(text, OWML.Common.MessageType.Info);
        }
    }
}
