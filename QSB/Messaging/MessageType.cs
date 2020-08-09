using UnityEngine.Networking;

namespace QSB.Messaging
{
    public enum MessageType
    {
        Sector = MsgType.Highest + 1,
        WakeUp = MsgType.Highest + 2,
        AnimTrigger = MsgType.Highest + 3,
        Join = MsgType.Highest + 4,
        Death = MsgType.Highest + 5,
        Leave = MsgType.Highest + 6,
        FullState = MsgType.Highest + 7,
        FullStateRequest = MsgType.Highest + 8,
        FlashlightActiveChange = MsgType.Highest + 9,
        SignalscopeActiveChange = MsgType.Highest + 10,
        TranslatorActiveChange = MsgType.Highest + 11,
        ProbeLauncherActiveChange = MsgType.Highest + 12,
        PlayerJoin = MsgType.Highest + 13,
        PlayerLeave = MsgType.Highest + 14,
        PlayerSectorChange = MsgType.Highest + 15
        // Add other message types here, incrementing the value.
    }
}
