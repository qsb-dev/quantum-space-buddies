using QSB.Messaging;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.ForestOfGalaxies.Messages;

internal class EyeCloneSeenMessage : QSBMessage
{

	public override bool ShouldReceive => QSBWorldSync.AllObjectsReady;

	public override void OnReceiveRemote()
	{
		var controller = QSBWorldSync.GetUnityObject<PlayerCloneController>();

		controller._warpFlickerActivated = true;
		controller._warpTime = Time.time + 0.5f;
	}
}