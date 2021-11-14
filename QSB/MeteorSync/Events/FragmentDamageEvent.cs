using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;
using EventType = QSB.Events.EventType;

namespace QSB.MeteorSync.Events
{
	public class FragmentDamageEvent : QSBEvent<FragmentDamageMessage>
	{
		public override EventType Type => EventType.FragmentDamage;

		public override void SetupListener()
			=> GlobalMessenger<QSBFragment, float>.AddListener(EventNames.QSBFragmentDamage, Handler);

		public override void CloseListener()
			=> GlobalMessenger<QSBFragment, float>.RemoveListener(EventNames.QSBFragmentDamage, Handler);

		private void Handler(QSBFragment qsbFragment, float damage) =>
			SendEvent(CreateMessage(qsbFragment, damage));

		private FragmentDamageMessage CreateMessage(QSBFragment qsbFragment, float damage) => new FragmentDamageMessage
		{
			ObjectId = qsbFragment.ObjectId,
			Damage = damage
		};

		public override void OnReceiveRemote(bool isHost, FragmentDamageMessage message)
		{
			if (!QSBCore.WorldObjectsReady)
			{
				return;
			}

			var qsbFragment = QSBWorldSync.GetWorldFromId<QSBFragment>(message.ObjectId);
			qsbFragment.AddDamage(message.Damage);
		}
	}
}
