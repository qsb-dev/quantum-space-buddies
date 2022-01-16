using QuantumUNET;

namespace QSB.Player
{
	public abstract class PlayerSyncObject : QNetworkBehaviour
	{
		protected uint PlayerId => NetId.Value;
		protected PlayerInfo Player => QSBPlayerManager.GetPlayer(PlayerId);
	}
}