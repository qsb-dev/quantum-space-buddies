using QSB.Events;
using QSB.Messaging;
using QSB.Utility;

namespace QSB.TransformSync
{
    public class PlayerSectorEvent : QSBEvent<SectorMessage>
    {
        public override MessageType Type => MessageType.PlayerSectorChange;

        public override void SetupListener()
        {
            GlobalMessenger<uint, Sector.Name, string>.AddListener(EventNames.QSBSectorChange, Handler);
        }
        
        public override void CloseListener()
        {
            GlobalMessenger<uint, Sector.Name, string>.RemoveListener(EventNames.QSBSectorChange, Handler);
        }

        private void Handler(uint netId, Sector.Name sectorId, string sectorName) => SendEvent(CreateMessage(netId, sectorId, sectorName));
        
        private SectorMessage CreateMessage(uint netId, Sector.Name sectorId, string sectorName) => new SectorMessage
        {
            SenderId = netId,
            SectorId = sectorId,
            SectorName = sectorName
        };

        public override void OnReceiveRemote(SectorMessage message)
        {
            if (!IsInUniverse)
            {
                return;
            }
            var sector = SectorSync.LocalInstance.FindSectorByName(message.SectorId, message.SectorName);

            if (sector == null)
            {
                DebugLog.ToScreen($"Sector {message.SectorName}, {message.SectorId} not found!");
                return;
            }

            var transformSync = PlayerRegistry.GetTransformSync(message.SenderId);
            DebugLog.ToScreen($"{transformSync.GetType().Name} of ID {message.SenderId} set to {message.SectorName}");
            UnityHelper.Instance.RunWhen(() => transformSync.SyncedTransform != null, 
                () => transformSync.SetReferenceSector(sector));
        }

    }
}
