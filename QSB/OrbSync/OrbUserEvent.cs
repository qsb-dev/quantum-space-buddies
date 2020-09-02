using QSB.Events;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.OrbSync
{
    public class OrbUserEvent : QSBEvent<WorldObjectMessage>
    {
        public override EventType Type => EventType.OrbUser;

        public override void SetupListener()
        {
            GlobalMessenger<int>.AddListener(EventNames.QSBOrbUser, Handler);
        }

        public override void CloseListener()
        {
            GlobalMessenger<int>.RemoveListener(EventNames.QSBOrbUser, Handler);
        }

        private void Handler(int id) => SendEvent(CreateMessage(id));

        private WorldObjectMessage CreateMessage(int id) => new WorldObjectMessage
        {
            AboutId = LocalPlayerId,
            ObjectId = id
        };

        public override void OnReceiveRemote(WorldObjectMessage message)
        {
            WorldRegistry.OrbUserList[WorldRegistry.OldOrbList[message.ObjectId]] = message.FromId;
        }

        public override void OnReceiveLocal(WorldObjectMessage message) => OnReceiveRemote(message);
    }
}
