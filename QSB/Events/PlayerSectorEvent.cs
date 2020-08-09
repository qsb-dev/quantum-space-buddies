using QSB.Messaging;

namespace QSB.Events
{
    class PlayerSectorEvent : QSBEvent<SectorMessage>
    {
        public override MessageType Type => MessageType.PlayerSectorChange;

        public override void SetupListener()
        {
            GlobalMessenger<uint, int, string>.AddListener("QSBPlayerSectorChange", (netId, id, name) => SendEvent(
                new SectorMessage {
                    SenderId = netId,
                    SectorId = id,
                    SectorName = name
                }));
        }

        public override void OnReceive(SectorMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}
