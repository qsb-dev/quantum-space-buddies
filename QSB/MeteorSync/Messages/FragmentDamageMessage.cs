using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;

namespace QSB.MeteorSync.Messages
{
	public class FragmentDamageMessage : QSBFloatWorldObjectMessage<QSBFragment>
	{
		public FragmentDamageMessage(float damage) => Value = damage;

		public FragmentDamageMessage() { }

		public override void OnReceiveRemote() => WorldObject.AddDamage(Value);
	}
}