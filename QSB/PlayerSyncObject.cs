using UnityEngine.Networking;

namespace QSB
{
    public abstract class PlayerSyncObject : NetworkBehaviour
    {
        public NetworkInstanceId NetId => GetComponent<NetworkIdentity>()?.netId ?? NetworkInstanceId.Invalid;
        public bool IsLocal => hasAuthority;
        private NetworkInstanceId PlayerId => this.GetPlayerOfObject();
        public PlayerInfo Player => PlayerRegistry.GetPlayer(PlayerId);
    }
}
