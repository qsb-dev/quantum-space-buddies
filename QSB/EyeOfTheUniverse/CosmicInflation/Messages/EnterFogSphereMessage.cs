using QSB.Messaging;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;

namespace QSB.EyeOfTheUniverse.CosmicInflation.Messages
{
	public class EnterFogSphereMessage : QSBMessage
	{
		public EnterFogSphereMessage()
		{
			DebugLog.DebugWrite("LOCAL enter fog sphere");
		}

		public override bool ShouldReceive => WorldObjectManager.AllObjectsReady;

		public override void OnReceiveRemote()
		{
			DebugLog.DebugWrite($"REMOTE enter fog sphere from {From}");
			var controller = QSBWorldSync.GetUnityObjects<CosmicInflationController>().First();

			controller._smokeSphereTrigger.SetTriggerActivation(false);
			controller._probeDestroyTrigger.SetTriggerActivation(false);
			controller.StartCollapse();
		}
	}
}
