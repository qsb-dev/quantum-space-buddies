using QSB.Utility;
using QuantumUNET;

namespace QSB.Player
{
	public abstract class PlayerSyncObject : QNetworkBehaviour
	{
		public uint AttachedNetId => NetIdentity?.NetId.Value ?? uint.MaxValue;
		public uint PlayerId => NetIdentity.RootIdentity?.NetId.Value ?? NetIdentity.NetId.Value;
		public PlayerInfo Player => PlayerManager.GetPlayer(PlayerId);

		protected virtual void Start() => PlayerManager.AddSyncObject(this);
		protected virtual void OnDestroy() => PlayerManager.RemoveSyncObject(this);
	}
}