using QSB.Events;
using QSB.MeteorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;
using EventType = QSB.Events.EventType;

namespace QSB.MeteorSync.Events
{
	public class MeteorImpactEvent : QSBEvent<MeteorImpactMessage>
	{
		public override EventType Type => EventType.MeteorImpact;

		public override void SetupListener()
			=> GlobalMessenger<int, Vector3, Quaternion, float>.AddListener(EventNames.QSBMeteorImpact, Handler);

		public override void CloseListener()
			=> GlobalMessenger<int, Vector3, Quaternion, float>.RemoveListener(EventNames.QSBMeteorImpact, Handler);

		private void Handler(int id, Vector3 pos, Quaternion rot, float damage) => SendEvent(CreateMessage(id, pos, rot, damage));

		private MeteorImpactMessage CreateMessage(int id, Vector3 pos, Quaternion rot, float damage) => new MeteorImpactMessage
		{
			ObjectId = id,
			Pos = pos,
			Rot = rot,
			Damage = damage
		};

		public override void OnReceiveRemote(bool isHost, MeteorImpactMessage message)
		{
			if (!MeteorManager.MeteorsReady)
			{
				return;
			}

			var qsbMeteor = QSBWorldSync.GetWorldFromId<QSBMeteor>(message.ObjectId);
			qsbMeteor.Impact(message.Pos, message.Rot, message.Damage);
		}
	}
}
