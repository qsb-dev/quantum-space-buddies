using QSB.Messaging;

namespace QSB.Events
{
    class PlayerSectorEvent : QSBEvent<SectorMessage>
    {
        public override MessageType Type => MessageType.PlayerSectorChange;

        public override void SetupListener()
        {
            throw new System.NotImplementedException();
        }

        public override void OnReceive(SectorMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}
