using QSB.Messaging;
using QSB.WorldSync;
using System.Linq;

namespace QSB.EchoesOfTheEye.Prisoner.Messages;

public class ProjectMessage : QSBMessage
{
	public override void OnReceiveRemote()
	{
		var director = QSBWorldSync.GetUnityObject<PrisonerDirector>();
		director._prisonerBrain.BeginBehavior(PrisonerBehavior.ExperienceVision, 0f);
	}
}
