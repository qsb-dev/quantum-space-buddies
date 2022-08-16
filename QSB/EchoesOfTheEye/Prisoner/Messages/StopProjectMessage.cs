using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.Prisoner.Messages;

internal class StopProjectMessage : QSBMessage<bool>
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
