using QSB.EchoesOfTheEye.PictureFrameDoors.Messages;
using QSB.Messaging;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EchoesOfTheEye.PictureFrameDoors.WorldObjects;

public abstract class QSBPictureFrameDoor<T> : WorldObject<T>, IQSBPictureFrameDoor
	where T : PictureFrameDoorInterface
{
	public abstract void SetOpenState(bool open);
}