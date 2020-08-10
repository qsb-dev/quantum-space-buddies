using QSB.Messaging;
using QSB.TransformSync;

namespace QSB.Events
{
    public class PlayerStatesRequestEvent : QSBEvent<PlayerMessage>
    {
        public override MessageType Type => MessageType.FullStateRequest;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener("QSBPlayerStatesRequest", () => SendEvent(
                CreateMessage()));
        }

        private PlayerMessage CreateMessage() => new PlayerMessage
        {
            SenderId = PlayerTransformSync.LocalInstance.netId.Value
        };

        public override void OnServerReceive(PlayerMessage message)
        {
            PlayerState.LocalInstance.Send();
        }
    }
}
