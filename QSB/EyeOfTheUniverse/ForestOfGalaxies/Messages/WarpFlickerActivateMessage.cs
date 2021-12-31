using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.ForestOfGalaxies.Messages
{
	internal class WarpFlickerActivateMessage : QSBMessage
	{
		public WarpFlickerActivateMessage()
		{
			DebugLog.DebugWrite("LOCAL player clone flicker");
		}

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			DebugLog.DebugWrite("REMOTE player clone flicker from {From");
			var controller = QSBWorldSync.GetUnityObjects<PlayerCloneController>().First();

			controller._warpFlickerActivated = true;
			controller._warpTime = Time.time + 0.5f;
		}
	}
}
