using Mirror;
using QSB.Messaging;
using QSB.Player;
using QSB.QuantumSync.WorldObjects;

namespace QSB.QuantumSync.Messages
{
	public class QuantumAuthorityMessage : QSBWorldObjectMessage<IQSBQuantumObject>
	{
		private uint AuthorityOwner;

		public QuantumAuthorityMessage(uint authorityOwner) => AuthorityOwner = authorityOwner;

		public override void Serialize(NetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(AuthorityOwner);
		}

		public override void Deserialize(NetworkReader reader)
		{
			base.Deserialize(reader);
			AuthorityOwner = reader.Read<uint>();
		}

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

				return (WorldObject.ControllingPlayer == 0 || AuthorityOwner == 0)
					&& WorldObject.ControllingPlayer != AuthorityOwner;
			}
		}

		public override void OnReceiveLocal() => WorldObject.ControllingPlayer = AuthorityOwner;

		public override void OnReceiveRemote()
		{
			WorldObject.ControllingPlayer = AuthorityOwner;
			if (WorldObject.ControllingPlayer == 00 && WorldObject.IsEnabled)
			{
				// object has no owner, but is still active for this player. request ownership
				WorldObject.SendMessage(new QuantumAuthorityMessage(QSBPlayerManager.LocalPlayerId));
			}
		}
	}
}
