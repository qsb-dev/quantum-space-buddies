using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.ForestOfGalaxies.Messages
{
	internal class EyeCloneSeenMessage : QSBMessage
	{

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			var controller = QSBWorldSync.GetUnityObjects<PlayerCloneController>().First();

			controller._warpFlickerActivated = true;
			controller._warpTime = Time.time + 0.5f;
		}
	}
}
