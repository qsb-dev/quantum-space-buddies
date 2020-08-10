using QSB.Messaging;
using QSB.TransformSync;

namespace QSB.Events
{
    class PlayerStatesRequestEvent : QSBEvent<PlayerMessage>
    {
        public override MessageType Type => MessageType.FullStateRequest;

        public override void SetupListener()
        {
            GlobalMessenger.AddListener("QSBPlayerStatesRequest", () => SendEvent(
                new PlayerMessage { 
                    SenderId = PlayerTransformSync.LocalInstance.netId.Value
                }));
        }

        public override void OnServerReceive(PlayerMessage message)
        {
            PlayerState.LocalInstance.Send();
        }
    }
}
