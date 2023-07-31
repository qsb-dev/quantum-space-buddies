using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

public class SetConcealedMessage : QSBWorldObjectMessage<QSBDreamLanternController, bool>
{
	public SetConcealedMessage(bool concealed) : base(concealed) { }

	public override void OnReceiveRemote() =>
		WorldObject.AttachedObject.SetConcealed(Data);
}
