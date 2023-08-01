using QSB.Messaging;
using QSB.WorldSync;
using System.Linq;

namespace QSB.EyeOfTheUniverse.Tomb.Messages;

public class EnterExitStageMessage : QSBMessage<bool>
{
	public EnterExitStageMessage(bool enter) : base(enter) { }

	public override void OnReceiveRemote()
	{
		var tomb = QSBWorldSync.GetUnityObjects<EyeTombController>().First();
		tomb._candleController.FadeTo(Data ? 1f : 0f, 1f);
	}
}
