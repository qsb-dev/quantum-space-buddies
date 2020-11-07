using OWML.Common;
using QSB.EventsCore;
using QSB.Utility;

namespace QSB.Player.Events
{
    public class PlayerJoinEvent : QSBEvent<PlayerJoinMessage>
    {
        public override EventType Type => EventType.PlayerJoin;

        public override void SetupListener() => GlobalMessenger<string>.AddListener(EventNames.QSBPlayerJoin, Handler);

        public override void CloseListener() => GlobalMessenger<string>.RemoveListener(EventNames.QSBPlayerJoin, Handler);

        private void Handler(string name) => SendEvent(CreateMessage(name));

        private PlayerJoinMessage CreateMessage(string name) => new PlayerJoinMessage
        {
            AboutId = LocalPlayerId,
            PlayerName = name
        };

        public override void OnReceiveRemote(PlayerJoinMessage message)
        {
            var player = QSBPlayerManager.GetPlayer(message.AboutId);
            player.Name = message.PlayerName;
            DebugLog.ToHud($"{player.Name} joined!");
            DebugLog.DebugWrite($"{player.Name} joined as id {player.PlayerId}", MessageType.Info);
        }

        public override void OnReceiveLocal(PlayerJoinMessage message)
        {
            var player = QSBPlayerManager.GetPlayer(QSBPlayerManager.LocalPlayerId);
            player.Name = message.PlayerName;
            var text = $"Connected to server as {player.Name}.";
            DebugLog.ToAll(text, MessageType.Info);
        }
    }
}
