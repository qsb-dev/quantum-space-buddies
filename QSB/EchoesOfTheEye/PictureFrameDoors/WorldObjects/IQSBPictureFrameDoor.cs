using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.PictureFrameDoors.WorldObjects;

public interface IQSBPictureFrameDoor : IWorldObject
{
	public void SetOpenState(bool open);
}
