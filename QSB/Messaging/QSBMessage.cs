using UnityEngine.Networking;

namespace QSB.Messaging
{
    public abstract class QSBMessage : MessageBase
    {
        public abstract MessageType MessageType { get; }
    }
}
