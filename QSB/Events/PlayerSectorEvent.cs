using QSB.Messaging;
using QSB.TransformSync;
using QSB.Utility;

namespace QSB.Events
{
    public class PlayerSectorEvent : QSBEvent<SectorMessage>
    {
        public override MessageType Type => MessageType.PlayerSectorChange;

        public override void SetupListener()
        {
            GlobalMessenger<uint, int, string>.AddListener("QSBSectorChange", (netId, id, name) => SendEvent(CreateMessage(netId, id, name)));
        }

        private SectorMessage CreateMessage(uint netId, int id, string name) => new SectorMessage
        {
            SenderId = netId,
            SectorId = id,
            SectorName = name
        };

        public override void OnReceiveRemote(SectorMessage message)
        {
            var sector = SectorSync.LocalInstance.FindSectorByName((Sector.Name)message.SectorId, message.SectorName);

            if (sector == null)
            {
                DebugLog.ToScreen($"Sector {message.SectorName},{(Sector.Name)message.SectorId} not found!");
                return;
            }

            var transformSync = PlayerRegistry.GetTransformSync(message.SenderId);
            DebugLog.ToScreen($"{transformSync.GetType().Name} of ID {message.SenderId} set to {message.SectorName}");
            transformSync.ReferenceTransform = sector.transform;
        }
    }
}
