using QSB.Messaging;
using QSB.WorldSync;
using System.Linq;

namespace QSB.EchoesOfTheEye.Prisoner.Messages;

public class EmergeTriggerMessage : QSBMessage
{
	public override void OnReceiveRemote()
	{
		// hewwo
		var director = QSBWorldSync.GetUnityObject<PrisonerDirector>();
		director._darknessAwoken = true;
		director._cellevator.OnPrisonerReveal();
		director._musicSource.SetLocalVolume(Locator.GetAudioManager().GetAudioEntry(director._musicSource.audioLibraryClip).volume);
		director._musicSource.Play();
		director._prisonerBrain.BeginBehavior(PrisonerBehavior.Emerge);
	}
}
