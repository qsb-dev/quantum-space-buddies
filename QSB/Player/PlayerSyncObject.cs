using QSB.QuantumUNET;

namespace QSB.Player
{
	public abstract class PlayerSyncObject : QSBNetworkBehaviour
	{
		public uint AttachedNetId => GetComponent<QSBNetworkIdentity>()?.NetId.Value ?? uint.MaxValue;
		public uint PlayerId => this.GetPlayerOfObject();
		public uint PreviousPlayerId { get; set; }
		public PlayerInfo Player => QSBPlayerManager.GetPlayer(PlayerId);
	}
}