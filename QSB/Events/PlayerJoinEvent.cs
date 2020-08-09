using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;

namespace QSB.Events
{
    class PlayerJoinEvent : QSBEvent<PlayerJoinMessage>
    {
        public override MessageType Type => MessageType.PlayerJoin;

        public override void SetupListener()
        {
            GlobalMessenger<string>.AddListener("QSBPlayerJoin", name => SendEvent(new PlayerJoinMessage { PlayerName = name }));
        }

        public override void OnReceive(uint sender, PlayerJoinMessage message)
        {
            var player = PlayerRegistry.CreatePlayer(sender);
            player.Name = message.PlayerName;
            player.IsReady = true;
            DebugLog.ToAll($"{player.Name} joined!");
        }

        public override void OnReceiveLocal(PlayerJoinMessage message)
        {
            var player = PlayerRegistry.CreatePlayer(PlayerTransformSync.LocalInstance.netId.Value);
            player.Name = message.PlayerName;
            player.IsReady = true;
            DebugLog.ToAll($"{player.Name} joined!");
        }
    }
}
