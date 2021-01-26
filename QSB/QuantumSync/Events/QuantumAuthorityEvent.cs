using QSB.Events;

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

		}

		public override void OnReceiveRemote(bool server, QuantumAuthorityMessage message)
		{

		}
	}
}