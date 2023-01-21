using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using UnityEngine;

namespace QSB.QuantumSync.Patches.Common;

[HarmonyPatch(typeof(QuantumMoon))]
internal class QuantumMoonPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(nameof(QuantumMoon.CheckPlayerFogProximity))]
	public static bool CheckPlayerFogProximity(QuantumMoon __instance)
	{
		var playerDistance = Vector3.Distance(__instance.transform.position, Locator.GetPlayerCamera().transform.position);
		var fogOffset = (__instance._stateIndex != 5) ? 0f : __instance._eyeStateFogOffset;
		var distanceFromFog = playerDistance - (__instance._fogRadius + fogOffset);
		var fogAlpha = 0f;
		if (!__instance._isPlayerInside)
		{
			fogAlpha = Mathf.InverseLerp(__instance._fogThickness + __instance._fogRolloffDistance, __instance._fogThickness, distanceFromFog);
			if (distanceFromFog < 0f)
			{
				if (__instance.IsLockedByProbeSnapshot() || QuantumManager.IsVisibleUsingCameraFrustum((ShapeVisibilityTracker)__instance._visibilityTracker, true).FoundPlayers)
				{
					__instance._isPlayerInside = true;
					__instance.SetSurfaceState(__instance._stateIndex);
					Locator.GetShipLogManager().RevealFact(__instance._revealFactID);
					GlobalMessenger.FireEvent(OWEvents.PlayerEnterQuantumMoon);
				}
				else
				{
					__instance.Collapse(true);
				}
			}
		}
		else if (__instance._isPlayerInside)
		{
			fogAlpha = Mathf.InverseLerp(-__instance._fogThickness - __instance._fogRolloffDistance, -__instance._fogThickness, distanceFromFog);
			if (distanceFromFog >= 0f)
			{
				if (__instance._stateIndex != 5)
				{
					__instance._isPlayerInside = false;
					if (!__instance.IsLockedByProbeSnapshot() && !QuantumManager.IsVisibleUsingCameraFrustum((ShapeVisibilityTracker)__instance._visibilityTracker, true).FoundPlayers)
					{
						__instance.Collapse(true);
					}

					__instance.SetSurfaceState(-1);
					GlobalMessenger.FireEvent(OWEvents.PlayerExitQuantumMoon);
				}
				else
				{
					var vector = Locator.GetPlayerTransform().position - __instance.transform.position;
					Locator.GetPlayerBody().SetVelocity(__instance._moonBody.GetPointVelocity(Locator.GetPlayerTransform().position) - (vector.normalized * 5f));
					var d = 80f;
					Locator.GetPlayerBody().SetPosition(__instance.transform.position + (__instance._vortexReturnPivot.up * d));
					if (!Physics.autoSyncTransforms)
					{
						Physics.SyncTransforms();
					}

					var component = Locator.GetPlayerCamera().GetComponent<PlayerCameraController>();
					component.SetDegreesY(component.GetMinDegreesY());
					__instance._vortexAudio.SetLocalVolume(0f);
					__instance._collapseToIndex = 1;
					__instance.Collapse(true);
				}
			}
		}

		__instance._playerFogBubble.SetFogAlpha(fogAlpha);
		__instance._shipLandingCamFogBubble.SetFogAlpha(fogAlpha);
		return false;
	}
}
