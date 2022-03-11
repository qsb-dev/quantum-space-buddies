namespace QSB.EchoesOfTheEye.PictureFrameDoors.WorldObjects;

internal class QSBPictureFrameDoorInterface : QSBPictureFrameDoor<PictureFrameDoorInterface>
{
	public override void SetOpenState(bool open)
	{
		if (AttachedObject._door.IsOpen() == open)
		{
			AttachedObject.UpdatePrompt();
			return;
		}

		if (open)
		{
			AttachedObject._door.Open();
		}
		else
		{
			AttachedObject._door.Close();
		}

		AttachedObject.UpdatePrompt();
	}
}
