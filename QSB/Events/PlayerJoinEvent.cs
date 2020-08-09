using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;

namespace QSB.Events
{
    class PlayerJoinEvent : QSBEvent
    {
        public override MessageType Type => MessageType.PlayerJoin;

        public override void SetupListener()
        {
            GlobalMessenger<string>.AddListener("QSBPlayerJoin", var => SendEvent(PlayerTransformSync.LocalInstance.netId.Value, var));
        }

        public override void OnReceive(uint sender, object[] data)
        {
            var player = PlayerRegistry.CreatePlayer(sender);
            player.Name = (string)data[0];
            player.IsReady = true;
            DebugLog.ToAll($"{player.Name} joined!");
        }

        public override void OnReceiveLocal(object[] data)
        {
            var player = PlayerRegistry.CreatePlayer(PlayerTransformSync.LocalInstance.netId.Value);
            player.Name = (string)data[0];
            player.IsReady = true;
            DebugLog.ToAll($"{player.Name} joined!");
        }
    }
}
