using QSB.Messaging;

namespace QSB.Events
{
    public class JoinMessage : NameMessage
    {
        public override MessageType MessageType => MessageType.Join;
    }
}
