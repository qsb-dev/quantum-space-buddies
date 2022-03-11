using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;

namespace QSB.MeteorSync.Messages;

public class FragmentIntegrityEvent : QSBWorldObjectMessage<QSBFragment, float>
{
	public FragmentIntegrityEvent(float integrity) : base(integrity) { }
	public override void OnReceiveRemote() => WorldObject.SetIntegrity(Data);
}
