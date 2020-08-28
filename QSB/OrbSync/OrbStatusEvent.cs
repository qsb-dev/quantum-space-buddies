using OWML.ModHelper.Events;
using QSB.Events;
using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.OrbSync
{
    public class OrbStatusEvent : QSBEvent<BoolWorldObjectMessage>
    {
        public override EventType Type => EventType.OrbStatus;

        public override void SetupListener()
        {
            GlobalMessenger<int, bool>.AddListener(EventNames.QSBOrbStatus, Handler);
        }

        public override void CloseListener()
        {
            GlobalMessenger<int, bool>.RemoveListener(EventNames.QSBOrbStatus, Handler);
        }

        private void Handler(int id, bool state) => SendEvent(CreateMessage(id, state));

        private BoolWorldObjectMessage CreateMessage(int id, bool state) => new BoolWorldObjectMessage
        {
            AboutId = LocalPlayerId,
            ObjectId = id,
            State = state
        };

        public override void OnReceiveRemote(BoolWorldObjectMessage message)
        {
            var orb = WorldRegistry.OldOrbList[message.ObjectId];
            orb.SetValue("_isBeingDragged", true);
        }
    }
}
