using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

internal class SetConcealedMessage : QSBWorldObjectMessage<QSBDreamLantern, bool>
{
	public SetConcealedMessage(bool concealed) : base(concealed) { }

	public override void OnReceiveRemote()
		=> QSBPatch.RemoteCall(() => WorldObject.AttachedObject.SetConcealed(Data));
}
