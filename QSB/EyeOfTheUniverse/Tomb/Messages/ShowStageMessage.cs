using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.EyeOfTheUniverse.Tomb.Messages;

internal class ShowStageMessage : QSBMessage
{
	public override void OnReceiveRemote()
	{
		var tomb = QSBWorldSync.GetUnityObject<EyeTombController>();
		tomb._stageRoot.SetActive(true);
	}
}
