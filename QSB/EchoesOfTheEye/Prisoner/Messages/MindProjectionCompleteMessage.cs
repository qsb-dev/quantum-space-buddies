using QSB.Messaging;
using QSB.WorldSync;

namespace QSB.EchoesOfTheEye.Prisoner.Messages;

internal class MindProjectionCompleteMessage : QSBMessage
{
	public override void OnReceiveRemote()
	{
		var director = QSBWorldSync.GetUnityObject<PrisonerDirector>();
		director.OnMindProjectionComplete();
	}
}
