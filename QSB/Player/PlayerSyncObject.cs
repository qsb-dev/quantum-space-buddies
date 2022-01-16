using Mirror;

namespace QSB.Player
{
	public abstract class PlayerSyncObject : NetworkBehaviour
	{
		protected uint PlayerId => netId;
		protected PlayerInfo Player => QSBPlayerManager.GetPlayer(PlayerId);
	}
}