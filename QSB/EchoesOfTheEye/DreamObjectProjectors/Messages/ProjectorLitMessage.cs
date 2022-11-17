using QSB.EchoesOfTheEye.DreamObjectProjectors.WorldObject;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.DreamObjectProjectors.Messages;

internal class ProjectorLitMessage : QSBWorldObjectMessage<QSBDreamObjectProjector, bool>
{
	public ProjectorLitMessage(bool lit) : base(lit) { }

	public override void OnReceiveRemote() => WorldObject.AttachedObject.SetLit(Data);
}
