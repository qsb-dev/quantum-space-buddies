using QSB.EchoesOfTheEye.PictureFrameDoors.WorldObjects;
using QSB.Messaging;

namespace QSB.EchoesOfTheEye.PictureFrameDoors.Messages;

public class PictureFrameDoorMessage : QSBWorldObjectMessage<IQSBPictureFrameDoor, bool>
{
	public PictureFrameDoorMessage(bool open) : base(open) { }

	public override void OnReceiveRemote()
		=> WorldObject.SetOpenState(Data);
}
