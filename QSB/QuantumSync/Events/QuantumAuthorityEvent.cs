using QSB.Events;
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

		public override void OnReceiveLocal(bool server, QuantumAuthorityMessage message)
		{
			DebugLog.DebugWrite($"Local set {message.ObjectId} to owner {message.AuthorityOwner}");
			var objects = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>();
			var obj = objects.First(x => (x as IWorldObject).ObjectId == message.ObjectId);
			obj.ControllingPlayer = message.AuthorityOwner;
		}

		public override void OnReceiveRemote(bool server, QuantumAuthorityMessage message)
		{
			var objects = QSBWorldSync.GetWorldObjects<IQSBQuantumObject>();
			var obj = objects.First(x => (x as IWorldObject).ObjectId == message.ObjectId);
			obj.ControllingPlayer = message.AuthorityOwner;
			DebugLog.DebugWrite($"Set {message.ObjectId} to owner {message.AuthorityOwner}");
		}
	}
}