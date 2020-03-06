using QSB.Messaging;

namespace QSB.Events
{
    public class LeaveMessage : NameMessage
    {
        public override MessageType MessageType => MessageType.Leave;
    }
}
