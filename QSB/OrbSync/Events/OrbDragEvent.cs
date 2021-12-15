using QSB.AuthoritySync;
using QSB.Events;
using QSB.WorldSync.Events;
using QSB.OrbSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.OrbSync.Events
{
	public class OrbDragEvent : QSBEvent<BoolWorldObjectMessage>
	{
		public override bool RequireWorldObjectsReady => true;

		public override void SetupListener() => GlobalMessenger<QSBOrb, bool>.AddListener(EventNames.QSBOrbDrag, Handler);
		public override void CloseListener() => GlobalMessenger<QSBOrb, bool>.RemoveListener(EventNames.QSBOrbDrag, Handler);

		private void Handler(QSBOrb qsbOrb, bool isDragging) => SendEvent(CreateMessage(qsbOrb, isDragging));

		private BoolWorldObjectMessage CreateMessage(QSBOrb qsbOrb, bool isDragging) => new()
		{
			ObjectId = qsbOrb.ObjectId,
			State = isDragging
		};

		public override void OnReceiveLocal(bool isHost, BoolWorldObjectMessage message)
		{
			var qsbOrb = QSBWorldSync.GetWorldFromId<QSBOrb>(message.ObjectId);

			if (message.State && isHost)
			{
				qsbOrb.TransformSync.NetIdentity.SetAuthority(message.FromId);
			}
		}

		public override void OnReceiveRemote(bool isHost, BoolWorldObjectMessage message)
		{
			var qsbOrb = QSBWorldSync.GetWorldFromId<QSBOrb>(message.ObjectId);

			if (message.State && isHost)
			{
				qsbOrb.TransformSync.NetIdentity.SetAuthority(message.FromId);
			}

			qsbOrb.IsBeingDragged = message.State;
		}
	}
}
