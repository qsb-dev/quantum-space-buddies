using OWML.Common;
using QSB.Events;
using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.TransformSync
{
    public class PlayerSectorEvent : QSBEvent<WorldObjectMessage>
    {
        public override EventType Type => EventType.PlayerSectorChange;

        public override void SetupListener()
        {
            GlobalMessenger<uint, QSBSector>.AddListener(EventNames.QSBSectorChange, Handler);
        }

        public override void CloseListener()
        {
            GlobalMessenger<uint, QSBSector>.RemoveListener(EventNames.QSBSectorChange, Handler);
        }

        private void Handler(uint netId, QSBSector sector) => SendEvent(CreateMessage(netId, sector));

        private WorldObjectMessage CreateMessage(uint netId, QSBSector sector) => new WorldObjectMessage
        {
            AboutId = netId,
            ObjectId = sector.ObjectId
        };

        public override void OnReceiveRemote(WorldObjectMessage message)
        {
            if (!QSBSceneManager.IsInUniverse)
            {
                //var player = PlayerRegistry.GetPlayer(message.FromId);
                //player.SectorCacheList[message.AboutId - message.FromId + 1] = message.ObjectId;
                return;
            }
            var sector = WorldRegistry.GetObject<QSBSector>(message.ObjectId);

            if (sector == null)
            {
                DebugLog.ToConsole($"Sector with order id {message.ObjectId} not found!", MessageType.Warning);
                return;
            }

            var transformSync = PlayerRegistry.GetSyncObject<TransformSync>(message.AboutId);

            QSB.Helper.Events.Unity.RunWhen(() => transformSync.SyncedTransform != null,
                () => transformSync.SetReferenceSector(sector));
        }

    }
}
