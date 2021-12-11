using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;

namespace QSB.MeteorSync.Events
{
	public class FragmentDamageMessage : QSBFloatWorldObjectMessage<QSBFragment>
	{
		public override void OnReceiveRemote() => WorldObject.AddDamage(Value);
	}
}
