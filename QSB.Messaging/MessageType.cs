using UnityEngine.Networking;

namespace QSB.Messaging
{
    public enum MessageType
    {
        SectorSync = MsgType.Highest + 1,
        WakeUpSync = MsgType.Highest + 2
    }
}
