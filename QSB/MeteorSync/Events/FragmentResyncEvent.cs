using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;
using EventType = QSB.Events.EventType;

namespace QSB.MeteorSync.Events
{
	/// called when we request a resync on client join
	public class FragmentResyncEvent : QSBEvent<FragmentResyncMessage>
	{
		public override EventType Type => EventType.FragmentResync;

		public override void SetupListener()
			=> GlobalMessenger<QSBFragment>.AddListener(EventNames.QSBFragmentResync, Handler);

		public override void CloseListener()
			=> GlobalMessenger<QSBFragment>.RemoveListener(EventNames.QSBFragmentResync, Handler);

		private void Handler(QSBFragment qsbFragment) => SendEvent(CreateMessage(qsbFragment));

		private FragmentResyncMessage CreateMessage(QSBFragment qsbFragment)
		{
			var msg = new FragmentResyncMessage
			{
				ObjectId = qsbFragment.ObjectId,
				Integrity = qsbFragment.AttachedObject._integrity,
				OrigIntegrity = qsbFragment.AttachedObject._origIntegrity
			};

			return msg;
		}

		public override void OnReceiveRemote(bool isHost, FragmentResyncMessage msg)
		{
			if (!MeteorManager.Ready)
			{
				return;
			}

			var qsbFragment = QSBWorldSync.GetWorldFromId<QSBFragment>(msg.ObjectId);
			qsbFragment.AttachedObject._integrity = msg.Integrity;
			qsbFragment.AttachedObject._origIntegrity = msg.OrigIntegrity;
			qsbFragment.AttachedObject.CallOnTakeDamage();
		}
	}
}
