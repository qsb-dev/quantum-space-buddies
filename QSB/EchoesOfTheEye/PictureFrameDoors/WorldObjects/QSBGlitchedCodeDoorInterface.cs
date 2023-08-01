namespace QSB.EchoesOfTheEye.PictureFrameDoors.WorldObjects;

public class QSBGlitchedCodeDoorInterface : QSBPictureFrameDoor<GlitchedCodeDoorInterface>
{
	public override void SetOpenState(bool open)
	{
		if (AttachedObject._door.IsOpen() == open)
		{
			AttachedObject.UpdatePrompt();
			AttachedObject.CheckPlayGlitchAudio();
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
		AttachedObject.CheckPlayGlitchAudio();
	}
}
