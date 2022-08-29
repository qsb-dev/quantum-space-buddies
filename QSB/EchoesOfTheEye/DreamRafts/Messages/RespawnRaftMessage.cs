using QSB.EchoesOfTheEye.DreamObjectProjectors.WorldObject;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.DreamRafts.Messages;

public class RespawnRaftMessage : QSBWorldObjectMessage<QSBDreamObjectProjector>
{
	public override void OnReceiveRemote()
	{
		var attachedObject = (DreamRaftProjector)WorldObject.AttachedObject;
		attachedObject.RespawnRaft();
	}
}
