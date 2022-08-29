using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

internal class SetLitMessage : QSBWorldObjectMessage<QSBDreamLantern, bool>
{
	public SetLitMessage(bool lit) : base(lit) { }

	public override void OnReceiveRemote()
		=> WorldObject.AttachedObject.SetLit(Data);
}
