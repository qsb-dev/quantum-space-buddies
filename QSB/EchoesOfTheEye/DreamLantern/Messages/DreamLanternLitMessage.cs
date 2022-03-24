using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

internal class DreamLanternLitMessage : QSBWorldObjectMessage<QSBDreamLanternItem, bool>
{
	public DreamLanternLitMessage(bool lit) : base(lit) { }

	public override void OnReceiveRemote()
		=> QSBPatch.RemoteCall(() => WorldObject.AttachedObject.SetLit(Data));
}
