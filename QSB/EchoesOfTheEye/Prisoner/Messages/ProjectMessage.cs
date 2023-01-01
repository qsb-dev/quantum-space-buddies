using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.Prisoner.Messages;

internal class ProjectMessage : QSBMessage
{
	public override void OnReceiveRemote()
	{
		var director = QSBWorldSync.GetUnityObject<PrisonerDirector>();
		director._prisonerBrain.BeginBehavior(PrisonerBehavior.ExperienceVision, 0f);
	}
}
