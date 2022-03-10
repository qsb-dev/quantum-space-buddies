using QSB.EchoesOfTheEye.PictureFrameDoors.Messages;
using QSB.Messaging;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.PictureFrameDoors.WorldObjects;

public abstract class QSBPictureFrameDoor<T> : WorldObject<T>, IQSBPictureFrameDoor
	where T : MonoBehaviour
{
	public override void SendInitialState(uint to)
		=> (this as IQSBPictureFrameDoor).SendMessage(new PictureFrameDoorMessage((AttachedObject as PictureFrameDoorInterface)._door.IsOpen()));

	public abstract void SetOpenState(bool open);
}