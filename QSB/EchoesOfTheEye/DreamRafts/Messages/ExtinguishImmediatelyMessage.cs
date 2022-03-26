using QSB.EchoesOfTheEye.DreamRafts.WorldObjects;
using QSB.Messaging;
using QSB.Patches;

namespace QSB.EchoesOfTheEye.DreamRafts.Messages;

public class ExtinguishImmediatelyMessage : QSBWorldObjectMessage<QSBDreamObjectProjector>
{
	public override void OnReceiveRemote()
	{
		var attachedObject = (DreamRaftProjector)WorldObject.AttachedObject;
		QSBPatch.RemoteCall(attachedObject.ExtinguishImmediately);
	}
}
