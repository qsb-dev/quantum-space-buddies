using QSB.Messaging;

namespace QSB.Events
{
    public class StateRequestMessage : PlayerMessage
    {
        public override MessageType MessageType => MessageType.FullStateRequest;
    }
}
