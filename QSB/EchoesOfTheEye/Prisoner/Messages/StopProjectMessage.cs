using QSB.Messaging;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.Prisoner.Messages;

public class StopProjectMessage : QSBMessage<bool>
{
	public StopProjectMessage(bool done) : base(done) { }

	public override void OnReceiveRemote()
	{
		var director = QSBWorldSync.GetUnityObject<PrisonerDirector>();

		if (!Data)
		{
			director._prisonerBrain.BeginBehavior(PrisonerBehavior.WaitForProjection, 0.5f);
			return;
		}

		director._prisonerDetector.SetActivation(false);
		director._prisonerBrain.BeginBehavior(PrisonerBehavior.ExperienceEmotionalCatharsis, 0.5f);
	}
}
