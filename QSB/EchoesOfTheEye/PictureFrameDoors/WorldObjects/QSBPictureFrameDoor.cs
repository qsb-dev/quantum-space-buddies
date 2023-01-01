using QSB.EchoesOfTheEye.PictureFrameDoors.Messages;
using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.PictureFrameDoors.WorldObjects;

public abstract class QSBPictureFrameDoor<T> : WorldObject<T>, IQSBPictureFrameDoor
	where T : PictureFrameDoorInterface
{
	public override void SendInitialState(uint to)
		=> ((IQSBPictureFrameDoor)this).SendMessage(new PictureFrameDoorMessage(AttachedObject._door.IsOpen()) { To = to });

	public abstract void SetOpenState(bool open);
}