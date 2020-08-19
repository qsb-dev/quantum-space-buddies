using QSB.Messaging;
using QSB.Utility;
using System.Linq;

namespace QSB.Events
{
    public class PlayerLeaveEvent : QSBEvent<PlayerLeaveMessage>
    {
        public override MessageType Type => MessageType.PlayerLeave;

        public override void SetupListener()
        {
            GlobalMessenger<uint, uint[]>.AddListener(EventNames.QSBPlayerLeave, Handler);
        }

        public override void CloseListener()
        {
            GlobalMessenger<uint, uint[]>.RemoveListener(EventNames.QSBPlayerLeave, Handler);
        }

        private void Handler(uint playerId, uint[] netIds) => SendEvent(CreateMessage(playerId, netIds));

        private PlayerLeaveMessage CreateMessage(uint playerId, uint[] netIds) => new PlayerLeaveMessage
        {
            AboutId = playerId,
            NetIds = netIds
        };

        public override void OnReceiveRemote(PlayerLeaveMessage message)
        {
            var playerName = PlayerRegistry.GetPlayer(message.AboutId).Name;
            DebugLog.ToConsole($"{playerName} disconnected.", OWML.Common.MessageType.Info);
            PlayerRegistry.RemovePlayer(message.AboutId);
            message.NetIds.ToList().ForEach(netId => QSBNetworkManager.Instance.CleanupNetworkBehaviour(netId));
        }
    }
}
