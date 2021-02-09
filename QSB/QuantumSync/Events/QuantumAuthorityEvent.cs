using OWML.Common;
using QSB.Events;
using QSB.Player;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;

namespace QSB.QuantumSync.Events
{
	internal class QuantumAuthorityEvent : QSBEvent<QuantumAuthorityMessage>
	{
		public override EventType Type => EventType.QuantumAuthority;

		public override void SetupListener() => GlobalMessenger<int, uint>.AddListener(EventNames.QSBQuantumAuthority, Handler);
		public override void CloseListener() => GlobalMessenger<int, uint>.RemoveListener(EventNames.QSBQuantumAuthority, Handler);

		private void Handler(int objId, uint authorityOwner) => SendEvent(CreateMessage(objId, authorityOwner));

		private QuantumAuthorityMessage CreateMessage(int objId, uint authorityOwner) => new QuantumAuthorityMessage
		{
			AboutId = LocalPlayerId,
			ObjectId = objId,
			AuthorityOwner = authorityOwner
		};

		public override bool CheckMessage(bool isServer, QuantumAuthorityMessage message)
		{
			if (!QuantumManager.Instance.IsReady)
			{
				return false;
			}

			var objects = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>();
			var obj = objects.ToList()[message.ObjectId];

			// Deciding if to change the object's owner
			//		  Message
			//	   | = 0 | > 0 |
			// = 0 | Yes*| Yes |
			// > 0 | Yes | No  |
			// Obj
			// *Doesn't change anything,
			// so can be yes or no

			return obj.ControllingPlayer == 0 || message.AuthorityOwner == 0;
		}

		public override void OnReceiveLocal(bool server, QuantumAuthorityMessage message)
		{
			var objects = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>();
			var obj = objects.ToList()[message.ObjectId];
			obj.ControllingPlayer = message.AuthorityOwner;
		}

		public override void OnReceiveRemote(bool server, QuantumAuthorityMessage message)
		{
			var objects = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>();
			var obj = objects.ToList()[message.ObjectId];
			if (obj.ControllingPlayer != 0 && message.AuthorityOwner != 0)
			{
				DebugLog.ToConsole($"Warning - object {(obj as IWorldObject).Name} already has owner {obj.ControllingPlayer}, but trying to be replaced by {message.AuthorityOwner}!", MessageType.Warning);
			}
			obj.ControllingPlayer = message.AuthorityOwner;
			if (obj.ControllingPlayer == 0 && obj.IsEnabled)
			{
				// object has no owner, but is still active for this player. request ownership
				GlobalMessenger<int, uint>.FireEvent(EventNames.QSBQuantumAuthority, message.ObjectId, QSBPlayerManager.LocalPlayerId);
			}
		}
	}
}