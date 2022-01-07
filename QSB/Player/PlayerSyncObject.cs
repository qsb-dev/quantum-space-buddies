using QuantumUNET;

namespace QSB.Player
{
	public abstract class PlayerSyncObject : QNetworkBehaviour
	{
		public uint PlayerId => NetId.Value;
		public PlayerInfo Player => QSBPlayerManager.GetPlayer(PlayerId);
	}
}