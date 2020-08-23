using UnityEngine.Networking;

namespace QSB
{
    public abstract class PlayerSyncObject : NetworkBehaviour
    {
        protected abstract uint PlayerIdOffset { get; }
        public uint NetId => GetComponent<NetworkIdentity>()?.netId.Value ?? 0;
        public bool IsLocal => hasAuthority;
        public uint PlayerId => NetId - PlayerIdOffset;
        public PlayerInfo Player => PlayerRegistry.GetPlayer(PlayerId);
    }
}
