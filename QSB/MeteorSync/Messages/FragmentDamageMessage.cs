using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;

namespace QSB.MeteorSync.Messages;

public class FragmentDamageMessage : QSBWorldObjectMessage<QSBFragment, float>
{
	public FragmentDamageMessage(float damage) : base(damage) { }

	public override void OnReceiveRemote() => WorldObject.AddDamage(Data);
}