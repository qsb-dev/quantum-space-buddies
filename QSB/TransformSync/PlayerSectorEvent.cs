using QSB.Events;
using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.TransformSync
{
    public class PlayerSectorEvent : QSBEvent<WorldObjectMessage>
    {
        public override MessageType Type => MessageType.PlayerSectorChange;

        public override void SetupListener()
        {
            GlobalMessenger<uint, QSBSector>.AddListener(EventNames.QSBSectorChange, (netId, sector) => SendEvent(CreateMessage(netId, sector)));
        }

        private WorldObjectMessage CreateMessage(uint netId, QSBSector sector) => new WorldObjectMessage
        {
            SenderId = netId,
            ObjectId = sector.ObjectId
        };

        public override void OnReceiveRemote(WorldObjectMessage message)
        {
            if (!IsInUniverse)
            {
                return;
            }
            var sector = WorldRegistry.GetObject<QSBSector>(message.ObjectId);

            if (sector == null)
            {
                DebugLog.ToScreen($"Sector with order id {message.ObjectId} not found!");
                return;
            }

            var transformSync = PlayerRegistry.GetTransformSync(message.SenderId);
            DebugLog.ToScreen($"{transformSync.GetType().Name} of ID {message.SenderId} set to {sector.GOName}");
            UnityHelper.Instance.RunWhen(() => transformSync.SyncedTransform != null, 
                () => transformSync.SetReferenceSector(sector));
        }

    }
}
