using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;

namespace QSB.MeteorSync.Events
{
	public class MeteorSpecialImpactMessage : QSBWorldObjectMessage<QSBMeteor>
	{
		public override void OnReceiveRemote() => WorldObject.SpecialImpact();
	}
}
