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
                return;
            }
            var sector = WorldRegistry.GetObject<QSBSector>(message.ObjectId);

            if (sector == null)
            {
                DebugLog.ToConsole($"Sector with order id {message.ObjectId} not found!");
                return;
            }

            var transformSync = PlayerRegistry.GetTransformSync(message.AboutId);
            //DebugLog.ToConsole($"{transformSync.GetType().Name} of ID {message.AboutId} set to {sector.Name}");
            UnityHelper.Instance.RunWhen(() => transformSync.SyncedTransform != null, 
                () => transformSync.SetReferenceSector(sector));
        }

    }
}
