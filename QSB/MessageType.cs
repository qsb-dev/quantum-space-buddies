using UnityEngine.Networking;

namespace QSB {
    public class MessageType: MsgType {
        public const short Sector = MsgType.Highest + 1;
        public const short ChatMessage = MsgType.Highest + 2;
        // Add other message types here, incrementing the number.
    }
}
