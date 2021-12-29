using QSB.Messaging;
using QSB.Player.Messages;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.VesselSync.WorldObjects
{
	internal class QSBVesselWarpController : WorldObject<VesselWarpController>
	{
		public override void Init()
		{
			AttachedObject._cageTrigger.OnEntry += OnEntry;
			AttachedObject._cageTrigger.OnExit += OnExit;
		}

		private void OnEntry(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				DebugLog.DebugWrite($"On entry");
				new EnterLeaveMessage(Player.EnterLeaveType.EnterVesselCage).Send();
			}
		}

		private void OnExit(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				DebugLog.DebugWrite($"On exit");
				new EnterLeaveMessage(Player.EnterLeaveType.ExitVesselCage).Send();
			}
		}
	}
}
