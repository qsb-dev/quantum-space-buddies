using QSB.Messaging;
using QSB.WorldSync;
using System.Linq;

namespace QSB.EchoesOfTheEye.Prisoner.Messages;

public class MindProjectionCompleteMessage : QSBMessage
{
	public override void OnReceiveRemote()
	{
		var director = QSBWorldSync.GetUnityObject<PrisonerDirector>();
		director.OnMindProjectionComplete();
	}
}
