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
            GlobalMessenger<string>.AddListener(EventNames.QSBPlayerJoin, Handler);
        }

        public override void CloseListener()
        {
            GlobalMessenger<string>.RemoveListener(EventNames.QSBPlayerJoin, Handler);
        }

        private void Handler(string name) => SendEvent(CreateMessage(name));

        private PlayerJoinMessage CreateMessage(string name) => new PlayerJoinMessage
        {
            FromId = PlayerTransformSync.LocalInstance.netId.Value,
            AboutId = PlayerTransformSync.LocalInstance.netId.Value,
            PlayerName = name
        };

        public override void OnReceiveRemote(PlayerJoinMessage message)
        {
            var player = PlayerRegistry.CreatePlayer(message.AboutId);
            player.Name = message.PlayerName;
            var text = $"{player.Name} joined!";
            DebugLog.ToAll(OWML.Common.MessageType.Info, text);
        }

        public override void OnReceiveLocal(PlayerJoinMessage message)
        {
            var player = PlayerRegistry.CreatePlayer(PlayerTransformSync.LocalInstance.netId.Value);
            player.Name = message.PlayerName;
            var text = $"Connected to server as {player.Name}.";
            DebugLog.ToAll(OWML.Common.MessageType.Info, text);
        }
    }
}
