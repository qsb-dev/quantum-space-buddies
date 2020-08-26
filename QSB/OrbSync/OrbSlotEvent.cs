using QSB.Events;
using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.OrbSync
{
    public class OrbSlotEvent : QSBEvent<OrbSlotMessage>
    {
        public override EventType Type => EventType.OrbSlot;

        public override void SetupListener()
        {
            GlobalMessenger<int, bool>.AddListener(EventNames.QSBOrbSlot, Handler);
        }

        public override void CloseListener()
        {
            GlobalMessenger<int, bool>.RemoveListener(EventNames.QSBOrbSlot, Handler);
        }

        private void Handler(int id, bool state) => SendEvent(CreateMessage(id, state));

        private OrbSlotMessage CreateMessage(int id, bool state) => new OrbSlotMessage
        {
            AboutId = LocalPlayerId,
            ObjectId = id,
            State = state
        };

        public override void OnReceiveRemote(OrbSlotMessage message)
        {
           
            var orbSlot = WorldRegistry.GetObject<QSBOrbSlot>(message.ObjectId);
            DebugLog.ToConsole($"GET ORB MESSAGE {message.ObjectId} : {message.State}");
            orbSlot?.SetState(message.State);
        }
    }
}
