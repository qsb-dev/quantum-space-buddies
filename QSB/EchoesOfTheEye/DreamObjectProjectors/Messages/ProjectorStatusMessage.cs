using QSB.EchoesOfTheEye.DreamObjectProjectors.WorldObject;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.DreamObjectProjectors.Messages;

internal class ProjectorStatusMessage : QSBWorldObjectMessage<QSBDreamObjectProjector, bool>
{
	public ProjectorStatusMessage(bool lit) : base(lit) { }

	public override void OnReceiveRemote()
		=> WorldObject.AttachedObject.SetLit(Data);
}
