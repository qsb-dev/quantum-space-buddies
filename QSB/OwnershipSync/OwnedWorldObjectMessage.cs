using QSB.Messaging;
using QSB.Player;

namespace QSB.OwnershipSync;

/// <summary>
/// request or release ownership of a world object
/// </summary>
public class OwnedWorldObjectMessage : QSBWorldObjectMessage<IOwnedWorldObject, uint>
{
	public OwnedWorldObjectMessage(uint owner) : base(owner) { }

	public override bool ShouldReceive
	{
		get
		{
			if (!base.ShouldReceive)
			{
				return false;
			}

			// Deciding if to change the object's owner
			//		  Message
			//	   | = 0 | > 0 |
			// = 0 | No  | Yes |
			// > 0 | Yes | No  |
			// if Obj==Message then No
			// Obj

			return (WorldObject.Owner == 0 || Data == 0) && WorldObject.Owner != Data;
		}
	}

	public override void OnReceiveLocal() => WorldObject.Owner = Data;

	public override void OnReceiveRemote()
	{
		WorldObject.Owner = Data;
		if (WorldObject.Owner == 0 && WorldObject.CanOwn)
		{
			// object has no owner, but is still active for this player. request ownership
			WorldObject.SendMessage(new OwnedWorldObjectMessage(QSBPlayerManager.LocalPlayerId));
		}
	}
}
