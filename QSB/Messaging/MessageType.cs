using UnityEngine.Networking;

namespace QSB.Messaging
{
    public enum MessageType
    {
        Sector,
        WakeUp,
        AnimTrigger,
        FullState,
        FullStateRequest,
        FlashlightActiveChange,
        SignalscopeActiveChange,
        TranslatorActiveChange,
        ProbeLauncherActiveChange,
        PlayerJoin,
        PlayerLeave,
        PlayerDeath,
        PlayerSectorChange
    }
}
