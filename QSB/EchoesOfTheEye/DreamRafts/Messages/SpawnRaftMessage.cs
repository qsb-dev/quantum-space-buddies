using QSB.EchoesOfTheEye.DreamRafts.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.DreamRafts.Messages;

public class SpawnRaftMessage : QSBWorldObjectMessage<QSBDreamRaftProjector>
{
	public override void OnReceiveRemote() =>
		WorldObject.AttachedObject.RespawnRaft();
}
