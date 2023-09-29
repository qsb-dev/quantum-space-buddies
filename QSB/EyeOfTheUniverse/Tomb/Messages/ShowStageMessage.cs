using QSB.Messaging;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.Tomb.Messages;

public class ShowStageMessage : QSBMessage
{
	public override void OnReceiveRemote()
	{
		var tomb = QSBWorldSync.GetUnityObject<EyeTombController>();
		tomb._stageRoot.SetActive(true);
		Object.Destroy(tomb.GetComponent<EyeTombWatcher>());
	}
}
