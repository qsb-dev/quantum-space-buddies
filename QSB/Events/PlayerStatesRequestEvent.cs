using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;
using System.Linq;
using UnityEngine.Networking;

namespace QSB.Events
{
    public class PlayerStatesRequestEvent : QSBEvent<PlayerMessage>
    {
        public override EventType Type => EventType.FullStateRequest;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener(EventNames.QSBPlayerStatesRequest, Handler);
        }

        public override void CloseListener()
        {
            GlobalMessenger.RemoveListener(EventNames.QSBPlayerStatesRequest, Handler);
        }

        private void Handler() => SendEvent(CreateMessage());

        private PlayerMessage CreateMessage() => new PlayerMessage
        {
            AboutId = LocalPlayerId
        };

        public override void OnServerReceive(PlayerMessage message)
        {
            DebugLog.DebugWrite($"[S] Get state request from {message.FromId}");
            PlayerState.LocalInstance.Send();
            foreach (var item in PlayerRegistry.GetSyncObjects<TransformSync.TransformSync>()
                .Where(x => x != null && x.IsReady && x.ReferenceSector != null))
            {
                GlobalMessenger<uint, QSBSector>.FireEvent(EventNames.QSBSectorChange, item.netId.Value, item.ReferenceSector);
            }
        }
    }
}
