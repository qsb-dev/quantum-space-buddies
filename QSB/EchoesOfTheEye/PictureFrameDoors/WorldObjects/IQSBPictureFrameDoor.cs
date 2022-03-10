using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.PictureFrameDoors.WorldObjects;

internal interface IQSBPictureFrameDoor : IWorldObject
{
	public void SetOpenState(bool open);
}
