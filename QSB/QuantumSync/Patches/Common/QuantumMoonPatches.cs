using HarmonyLib;
using QSB.Messaging;
using QSB.Patches;
using QSB.Player;
using QSB.ShipSync;
using QSB.Utility;
using System.Linq;
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
		var fogOffset = (__instance._stateIndex == 5) ? __instance._eyeStateFogOffset : 0f;
		var distanceFromFog = playerDistance - (__instance._fogRadius + fogOffset);
		var fogAlpha = 0f;

		if (!__instance._isPlayerInside)
		{
			fogAlpha = Mathf.InverseLerp(__instance._fogThickness + __instance._fogRolloffDistance, __instance._fogThickness, distanceFromFog);
			if (distanceFromFog < 0f) // inside fog
			{
				var playersWhoCanSeeMoon = QuantumManager.IsVisibleUsingCameraFrustum((ShapeVisibilityTracker)__instance._visibilityTracker, true).PlayersWhoCanSee;
				var shipInFog = GetShipInFog(__instance);

				DebugLog.DebugWrite($"Inside Fog - shipInFog:{shipInFog} playersWhoCanSeeMoon.Count:{playersWhoCanSeeMoon.Count}, lockedByProbeSnapshot:{__instance.IsLockedByProbeSnapshot()}");

				if (playersWhoCanSeeMoon.Any(x => !(shipInFog && x.IsInShip) && !GetTransformInFog(__instance, x.CameraBody.transform)) || __instance.IsLockedByProbeSnapshot())
				{
					/* Either :
					 * - The moon is locked with a snapshot
					 * OR
					 * - If the ship is in the fog :
					 *    - there are people outside the ship who can see the moon, and who are not in the fog
					 * - If the ship is not in the fog
					 *    - There are people who can see the moon, who are not in the fog
					 */
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

	private static bool GetShipInFog(QuantumMoon moon)
	{
		if (ShipManager.Instance.IsShipWrecked)
		{
			return false;
		}	

		var distance = Vector3.Distance(moon.transform.position, Locator.GetShipTransform().position);
		var fogOffset = (moon._stateIndex == 5) ? moon._eyeStateFogOffset : 0f;
		var distanceFromFog = distance - (moon._fogRadius + fogOffset);
		return distanceFromFog < 10f;
	}

	private static bool GetTransformInFog(QuantumMoon moon, Transform transform)
	{
		var distance = Vector3.Distance(moon.transform.position, transform.position);
		var fogOffset = (moon._stateIndex == 5) ? moon._eyeStateFogOffset : 0f;
		var distanceFromFog = distance - (moon._fogRadius + fogOffset);
		return distanceFromFog < 0f;
	}
}
