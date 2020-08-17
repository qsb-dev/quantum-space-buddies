using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;

namespace QSB.Events
{
    public class PlayerJoinEvent : QSBEvent<PlayerJoinMessage>
    {
        public override MessageType Type => MessageType.PlayerJoin;

        public override void SetupListener()
        {
            GlobalMessenger<string>.AddListener(EventNames.QSBPlayerJoin, name => SendEvent(CreateMessage(name)));
        }

        private PlayerJoinMessage CreateMessage(string name) => new PlayerJoinMessage
        {
            SenderId = PlayerTransformSync.LocalInstance.netId.Value,
            PlayerName = name
        };

        public override void OnReceiveRemote(PlayerJoinMessage message)
        {
            var player = PlayerRegistry.GetPlayer(message.SenderId);
            player.Name = message.PlayerName;
            var text = $"{player.Name} joined!";
            DebugLog.ToAll(OWML.Common.MessageType.Info, text);
        }

        public override void OnReceiveLocal(PlayerJoinMessage message)
        {
            var player = PlayerRegistry.GetPlayer(PlayerTransformSync.LocalInstance.netId.Value);
            player.Name = message.PlayerName;
            var text = $"Connected to server as {player.Name}.";
            DebugLog.ToAll(OWML.Common.MessageType.Info, text);
        }
    }
}
