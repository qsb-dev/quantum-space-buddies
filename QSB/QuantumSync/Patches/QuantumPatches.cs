using QSB.Events;
using QSB.Patches;
using UnityEngine;

namespace QSB.QuantumSync.Patches
{
	public class QuantumPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			QSBCore.Helper.HarmonyHelper.AddPrefix<QuantumShrine>("OnEntry", typeof(QuantumPatches), nameof(Shrine_OnEntry));
			QSBCore.Helper.HarmonyHelper.AddPrefix<QuantumShrine>("OnExit", typeof(QuantumPatches), nameof(Shrine_OnExit));
		}

		public static bool Shrine_OnEntry(
			GameObject hitObj,
			ref bool ____isPlayerInside,
			ref bool ____fading,
			OWLightController ____exteriorLightController,
			ref bool ____isProbeInside)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				____isPlayerInside = true;
				____fading = true;
				____exteriorLightController.FadeTo(0f, 1f);
				GlobalMessenger.FireEvent(EventNames.QSBEnterShrine);
			}
			else if (hitObj.CompareTag("ProbeDetector"))
			{
				____isProbeInside = true;
			}
			return false;
		}

		public static bool Shrine_OnExit(
			GameObject hitObj,
			ref bool ____isPlayerInside,
			ref bool ____fading,
			OWLightController ____exteriorLightController,
			ref bool ____isProbeInside)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				____isPlayerInside = false;
				____fading = true;
				____exteriorLightController.FadeTo(1f, 1f);
				GlobalMessenger.FireEvent(EventNames.QSBExitShrine);
			}
			else if (hitObj.CompareTag("ProbeDetector"))
			{
				____isProbeInside = false;
			}
			return false;
		}
	}
}
