using QSB.Messaging;
using QSB.MeteorSync.WorldObjects;

namespace QSB.MeteorSync.Messages;

public class FragmentIntegrityMessage : QSBWorldObjectMessage<QSBFragment, float>
{
	public FragmentIntegrityMessage(float integrity) : base(integrity) { }
	public override void OnReceiveRemote() => WorldObject.SetIntegrity(Data);
}
