using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.DreamLantern.Messages;

internal class SetConcealedMessage : QSBWorldObjectMessage<QSBDreamLantern, bool>
{
	public SetConcealedMessage(bool concealed) : base(concealed) { }

	public override void OnReceiveRemote()
		=> WorldObject.AttachedObject.SetConcealed(Data);
}
