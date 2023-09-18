using QSB.Messaging;
using QSB.Player;

namespace QSB.OwnershipSync;

/// <summary>
/// request or release ownership of a world object
/// </summary>
public class OwnedWorldObjectMessage : QSBWorldObjectMessage<IOwnedWorldObject, uint>
{
	public OwnedWorldObjectMessage(uint owner) : base(owner) { }

	public override void OnReceiveLocal() => WorldObject.Owner = Data;

	public override void OnReceiveRemote()
	{
		WorldObject.Owner = Data;
		if (WorldObject.Owner == 0 && WorldObject.CanOwn)
		{
			// object has no owner, but is still active for this player. request ownership
			// means you should wait and check after releasing ownership in case someone else gets it
			WorldObject.SendMessage(new OwnedWorldObjectMessage(QSBPlayerManager.LocalPlayerId));
		}
	}
}
