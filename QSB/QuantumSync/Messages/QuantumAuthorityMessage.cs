using QSB.Messaging;
using QSB.Player;
using QSB.QuantumSync.WorldObjects;

namespace QSB.QuantumSync.Messages;

public class QuantumAuthorityMessage : QSBWorldObjectMessage<IQSBQuantumObject, uint>
{
	public QuantumAuthorityMessage(uint authorityOwner) => Value = authorityOwner;

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

			return (WorldObject.ControllingPlayer == 0 || Value == 0)
			       && WorldObject.ControllingPlayer != Value;
		}
	}

	public override void OnReceiveLocal() => WorldObject.ControllingPlayer = Value;

	public override void OnReceiveRemote()
	{
		WorldObject.ControllingPlayer = Value;
		if (WorldObject.ControllingPlayer == 00 && WorldObject.IsEnabled)
		{
			// object has no owner, but is still active for this player. request ownership
			WorldObject.SendMessage(new QuantumAuthorityMessage(QSBPlayerManager.LocalPlayerId));
		}
	}
}