using QSB.Messaging;
using QSB.Player;
using QSB.QuantumSync.WorldObjects;

namespace QSB.QuantumSync.Messages;

public class QuantumOwnershipMessage : QSBWorldObjectMessage<IQSBQuantumObject, uint>
{
	public QuantumOwnershipMessage(uint owner) : base(owner) { }

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

			return (WorldObject.ControllingPlayer == 0 || Data == 0)
			       && WorldObject.ControllingPlayer != Data;
		}
	}

	public override void OnReceiveLocal() => WorldObject.ControllingPlayer = Data;

	public override void OnReceiveRemote()
	{
		WorldObject.ControllingPlayer = Data;
		if (WorldObject.ControllingPlayer == 00 && WorldObject.IsEnabled)
		{
			// object has no owner, but is still active for this player. request ownership
			WorldObject.SendMessage(new QuantumOwnershipMessage(QSBPlayerManager.LocalPlayerId));
		}
	}
}