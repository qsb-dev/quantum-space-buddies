using QSB.Messaging;
using QSB.Player;
using QSB.QuantumSync.WorldObjects;
using QuantumUNET.Transport;

namespace QSB.QuantumSync.Events
{
	public class QuantumAuthorityMessage : QSBWorldObjectMessage<IQSBQuantumObject>
	{
		public uint AuthorityOwner;

		public override void Deserialize(QNetworkReader reader)
		{
			base.Deserialize(reader);
			AuthorityOwner = reader.ReadUInt32();
		}

		public override void Serialize(QNetworkWriter writer)
		{
			base.Serialize(writer);
			writer.Write(AuthorityOwner);
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
				WorldObject.SendMessage(new QuantumAuthorityMessage
				{
					AuthorityOwner = QSBPlayerManager.LocalPlayerId
				});
			}
		}
	}
}
