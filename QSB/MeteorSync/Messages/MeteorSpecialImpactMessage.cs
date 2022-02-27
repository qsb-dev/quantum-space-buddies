using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;

namespace QSB.MeteorSync.Messages
{
	public class MeteorSpecialImpactMessage : QSBWorldObjectMessage<QSBMeteor>
	{
		public override void OnReceiveRemote() => WorldObject.SpecialImpact();
	}
}