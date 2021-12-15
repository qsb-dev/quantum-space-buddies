using QSB.AuthoritySync;
using QSB.Events;
using QSB.WorldSync.Events;
using QSB.OrbSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.OrbSync.Events
{
	public class OrbUserEvent : QSBEvent<BoolWorldObjectMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<QSBOrb, bool>.AddListener(EventNames.QSBOrbUser, Handler);
		public override void CloseListener() => GlobalMessenger<QSBOrb, bool>.RemoveListener(EventNames.QSBOrbUser, Handler);

		private void Handler(QSBOrb qsbOrb, bool isDragging) => SendEvent(CreateMessage(qsbOrb, isDragging));

		private BoolWorldObjectMessage CreateMessage(QSBOrb qsbOrb, bool isDragging) => new()
		{
			ObjectId = qsbOrb.ObjectId,
			State = isDragging
		};

		public override void OnReceiveLocal(bool isServer, BoolWorldObjectMessage message) => OnReceive(false, isServer, message);
		public override void OnReceiveRemote(bool isServer, BoolWorldObjectMessage message) => OnReceive(true, isServer, message);

		private static void OnReceive(bool isRemote, bool isServer, BoolWorldObjectMessage message)
		{
			var qsbOrb = QSBWorldSync.GetWorldFromId<QSBOrb>(message.ObjectId);

			if (message.State && isServer)
			{
				qsbOrb.TransformSync.NetIdentity.SetAuthority(message.FromId);
			}
			if (isRemote)
			{
				qsbOrb.IsBeingDragged = message.State;
			}
		}
	}
}
