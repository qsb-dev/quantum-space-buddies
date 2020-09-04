using System;
using UnityEngine.Networking;

namespace QSB
{
    public abstract class PlayerSyncObject : NetworkBehaviour
    {
        public NetworkInstanceId NetId => GetComponent<NetworkIdentity>()?.netId ?? NetworkInstanceId.Invalid;
        public NetworkInstanceId PlayerId => this.GetPlayerOfObject();
        public NetworkInstanceId PreviousPlayerId { get; set; }
        public PlayerInfo Player => PlayerRegistry.GetPlayer(PlayerId);
    }
}
