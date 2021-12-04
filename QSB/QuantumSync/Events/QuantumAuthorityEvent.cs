using QSB.Events;
using QSB.Player;
using QSB.WorldSync;

namespace QSB.QuantumSync.Events
{
	internal class QuantumAuthorityEvent : QSBEvent<QuantumAuthorityMessage>
	{
		public override void SetupListener() => GlobalMessenger<int, uint>.AddListener(EventNames.QSBQuantumAuthority, Handler);
		public override void CloseListener() => GlobalMessenger<int, uint>.RemoveListener(EventNames.QSBQuantumAuthority, Handler);

		private void Handler(int objId, uint authorityOwner) => SendEvent(CreateMessage(objId, authorityOwner));

		private QuantumAuthorityMessage CreateMessage(int objId, uint authorityOwner) => new()
		{
			AboutId = LocalPlayerId,
			ObjectId = objId,
			AuthorityOwner = authorityOwner
		};

		public override bool CheckMessage(bool isServer, QuantumAuthorityMessage message)
		{
			if (!WorldObjectManager.AllObjectsReady)
			{
				return false;
			}

			var obj = QSBWorldSync.GetWorldFromId<IQSBQuantumObject>(message.ObjectId);

			// Deciding if to change the object's owner
			//		  Message
			//	   | = 0 | > 0 |
			// = 0 | No  | Yes |
			// > 0 | Yes | No  |
			// if Obj==Message then No
			// Obj

			return (obj.ControllingPlayer == 0 || message.AuthorityOwner == 0)
				&& (obj.ControllingPlayer != message.AuthorityOwner);
		}

		public override void OnReceiveLocal(bool server, QuantumAuthorityMessage message)
		{
			var obj = QSBWorldSync.GetWorldFromId<IQSBQuantumObject>(message.ObjectId);
			obj.ControllingPlayer = message.AuthorityOwner;
		}

		public override void OnReceiveRemote(bool server, QuantumAuthorityMessage message)
		{
			var obj = QSBWorldSync.GetWorldFromId<IQSBQuantumObject>(message.ObjectId);
			obj.ControllingPlayer = message.AuthorityOwner;
			if (obj.ControllingPlayer == 0 && obj.IsEnabled)
			{
				// object has no owner, but is still active for this player. request ownership
				QSBEventManager.FireEvent(EventNames.QSBQuantumAuthority, message.ObjectId, QSBPlayerManager.LocalPlayerId);
			}
		}
	}
}