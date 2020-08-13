using QSB.Events;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.GeyserSync
{
    public class GeyserEvent : QSBEvent<GeyserMessage>
    {
        public override MessageType Type => MessageType.Geyser;

        public static SyncObjects ObjectType => SyncObjects.Geysers;

        public override void SetupListener()
        {
            GlobalMessenger<GeyserController, bool>.AddListener(EventNames.QSBGeyserState, (controller, state) => SendEvent(CreateMessage(controller, state)));
        }

        private GeyserMessage CreateMessage(GeyserController controller, bool state) => new GeyserMessage
        {
            SenderId = PlayerRegistry.LocalPlayer.NetId,
            ObjectID = WorldRegistry.GetObjectID(ObjectType, controller),
            ObjectType = ObjectType,
            State = state
        };

        public override void OnReceiveRemote(GeyserMessage message)
        {
            if (!IsInUniverse)
            {
                return;
            }
            if (message.State)
            {
                WorldRegistry.GeyserControllers[message.ObjectID].ActivateGeyser();
            }
            else
            {
                WorldRegistry.GeyserControllers[message.ObjectID].DeactivateGeyser();
            }
        }
    }
}
