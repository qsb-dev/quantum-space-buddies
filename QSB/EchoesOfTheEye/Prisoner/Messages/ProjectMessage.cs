using QSB.Messaging;
using QSB.WorldSync;
using System.Linq;

namespace QSB.EchoesOfTheEye.Prisoner.Messages;

internal class ProjectMessage : QSBMessage
{
	public override void OnReceiveRemote()
	{
		var director = QSBWorldSync.GetUnityObjects<PrisonerDirector>().First();
		director._prisonerBrain.BeginBehavior(PrisonerBehavior.ExperienceVision, 0f);
	}
}
