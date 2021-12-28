using QuantumUNET;

namespace QSB.Player
{
	public abstract class PlayerSyncObject : QNetworkBehaviour
	{
		public uint PlayerId => NetIdentity.NetId.Value;
		public PlayerInfo Player => QSBPlayerManager.GetPlayer(PlayerId);
	}
}