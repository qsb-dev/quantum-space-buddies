using System;
using OWML.Common;
using QSB.Utility;
using UnityEngine.Networking;

namespace QSB
{
    public abstract class PlayerSyncObject : NetworkBehaviour
    {
        protected abstract uint PlayerIdOffset { get; }

        public uint NetId => netId.Value;
        public bool IsLocal => hasAuthority;
        public uint PlayerId => GetPlayerId();
        public PlayerInfo Player => PlayerRegistry.GetPlayer(PlayerId);

        private uint GetPlayerId()
        {
            try
            {
                return NetId - PlayerIdOffset;
            }
            catch
            {
                DebugLog.ToConsole($"Error while getting netId of {GetType().Name}! " +
                                   $"{Environment.NewLine}     - Did you destroy the TransformSync without destroying the {GetType().Name}?" +
                                   $"{Environment.NewLine}     - Did a destroyed TransformSync/{GetType().Name} still have an active action/event listener?" +
                                   $"{Environment.NewLine}     If you are a user seeing this, please report this error.", MessageType.Error);
                return uint.MaxValue;
            }
        }

    }
}
