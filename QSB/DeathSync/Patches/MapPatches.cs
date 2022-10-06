using HarmonyLib;
using QSB.Patches;
using UnityEngine;

namespace QSB.DeathSync.Patches;

[HarmonyPatch]
internal class MapPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.SpectateTime;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MapController), nameof(MapController.EnterMapView))]
	public static bool MapController_EnterMapView(
		MapController __instance
	)
	{
		if (__instance._isMapMode)
		{
			return false;
		}

		__instance._mapMarkerManager.SetVisible(true);
		GlobalMessenger.FireEvent("EnterMapView");
		GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", __instance._mapCamera);
		if (__instance._audioSource.isPlaying)
		{
			__instance._audioSource.Stop();
			__instance._audioSource.SetLocalVolume(1f);
			__instance._audioSource.Play();
		}
		else
		{
			__instance._audioSource.SetLocalVolume(1f);
			__instance._audioSource.Play();
		}

		Locator.GetAudioMixer().MixMap();
		__instance._activeCam.enabled = false;
		__instance._mapCamera.enabled = true;
		__instance._gridRenderer.enabled = false;
		__instance._targetTransform = null;
		__instance._lockedToTargetTransform = false;
		__instance._position = RespawnOnDeath.Instance.DeathPositionWorld - Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().GetPosition();
		__instance._position.y = 0f;
		__instance._yaw = __instance._defaultYawAngle;
		__instance._pitch = __instance._initialPitchAngle;
		__instance._zoom = __instance._initialZoomDist;
		__instance._targetZoom = __instance._defaultZoomDist;
		__instance.transform.rotation = Quaternion.LookRotation(-RespawnOnDeath.Instance.DeathPlayerUpVector, RespawnOnDeath.Instance.DeathPlayerForwardVector);
		__instance.transform.position = RespawnOnDeath.Instance.DeathPositionWorld;
		__instance._interpPosition = true;
		__instance._interpPitch = true;
		__instance._interpZoom = true;
		__instance._framingPlayer = __instance._lockedToTargetTransform;
		__instance._lockTimer = __instance._lockOnMoveLength;
		__instance._gridOverrideSize = (__instance._currentRFrame == null) ? 0f : __instance._currentRFrame.GetAutopilotArrivalDistance();
		__instance._gridOverride = __instance._gridOverrideSize > 0f;
		__instance._gridTimer = (!__instance._gridOverride) ? 0f : __instance._gridLockOnLength;
		__instance._revealLength = 20f;
		__instance._revealTimer = 0f;
		__instance._isMapMode = true;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MapController), nameof(MapController.LateUpdate))]
	public static bool MapController_LateUpdate(
		MapController __instance
	)
	{
		__instance._lockTimer = Mathf.Min(__instance._lockTimer + Time.deltaTime, __instance._lockOnMoveLength);
		__instance._revealTimer = Mathf.Min(__instance._revealTimer + Time.deltaTime, __instance._revealLength);

		var revealFraction = Mathf.Clamp01(__instance._revealTimer / __instance._revealLength);
		var smoothedRevealFraction = Mathf.SmoothStep(0f, 1f, revealFraction);

		var canInteractWith = __instance._revealTimer > 18f;

		if (__instance._screenPromptsVisible && __instance._isPaused)
		{
			__instance._closePrompt.SetVisibility(false);
			__instance._panPrompt.SetVisibility(false);
			__instance._rotatePrompt.SetVisibility(false);
			__instance._zoomPrompt.SetVisibility(false);
			__instance._screenPromptsVisible = false;
		}
		else if (!__instance._screenPromptsVisible && canInteractWith && !__instance._isPaused)
		{
			__instance._closePrompt.SetVisibility(false);
			__instance._panPrompt.SetVisibility(true);
			__instance._rotatePrompt.SetVisibility(true);
			__instance._zoomPrompt.SetVisibility(true);
			__instance._screenPromptsVisible = true;
		}

		var XZinput = Vector2.zero;
		var lookInput = Vector2.zero;
		var zoomInput = 0f;
		if (canInteractWith)
		{
			XZinput = OWInput.GetAxisValue(InputLibrary.moveXZ);
			lookInput = InputLibrary.look.GetAxisValue(false);
			zoomInput = OWInput.GetValue(InputLibrary.mapZoomIn) - OWInput.GetValue(InputLibrary.mapZoomOut);
			lookInput.y *= -1f;
			zoomInput *= -1f;
		}

		__instance._lockedToTargetTransform &= XZinput.sqrMagnitude < 0.01f;
		__instance._interpPosition &= XZinput.sqrMagnitude < 0.01f;
		__instance._interpPitch &= Mathf.Abs(lookInput.y) < 0.1f;
		__instance._interpZoom &= Mathf.Abs(zoomInput) < 0.1f;

		if (__instance._interpPosition)
		{
			var a = __instance._activeCam.transform.position - Locator.GetCenterOfTheUniverse().GetOffsetPosition();
			var b = Vector3.zero;
			__instance._position = Vector3.Lerp(a, b, smoothedRevealFraction);
		}
		else
		{
			var normalized = Vector3.Scale(__instance.transform.forward + __instance.transform.up, new Vector3(1f, 0f, 1f)).normalized;
			var a2 = (__instance.transform.right * XZinput.x) + (normalized * XZinput.y);
			__instance._position += a2 * __instance._panSpeed * __instance._zoom * Time.deltaTime;
			__instance._position.y = 0f;
			if (__instance._position.sqrMagnitude > __instance._maxPanDistance * __instance._maxPanDistance)
			{
				__instance._position = __instance._position.normalized * __instance._maxPanDistance;
			}
		}

		__instance._yaw += lookInput.x * __instance._yawSpeed * Time.deltaTime;
		__instance._yaw = OWMath.WrapAngle(__instance._yaw);
		if (__instance._interpPitch)
		{
			__instance._pitch = Mathf.Lerp(__instance._initialPitchAngle, __instance._defaultPitchAngle, smoothedRevealFraction);
		}
		else
		{
			__instance._pitch += lookInput.y * __instance._pitchSpeed * Time.deltaTime;
			__instance._pitch = Mathf.Clamp(__instance._pitch, __instance._minPitchAngle, __instance._maxPitchAngle);
		}

		if (__instance._interpZoom)
		{
			__instance._zoom = Mathf.Lerp(__instance._initialZoomDist, __instance._targetZoom, smoothedRevealFraction);
		}
		else
		{
			__instance._zoom += zoomInput * __instance._zoomSpeed * Time.deltaTime;
			__instance._zoom = Mathf.Clamp(__instance._zoom, __instance._minZoomDistance, __instance._maxZoomDistance);
		}

		__instance._mapCamera.nearClipPlane = Mathf.Lerp(0.1f, 1f, smoothedRevealFraction);

		var finalRotation = Quaternion.Euler(__instance._pitch, __instance._yaw, 0f);

		var num4 = revealFraction * (2f - revealFraction);

		var num5 = Mathf.SmoothStep(0f, 1f, num4);

		// Create rotation that's looking down at the player from above
		var lookingDownAtPlayer = Quaternion.LookRotation(-RespawnOnDeath.Instance.DeathPlayerUpVector, Vector3.up);

		// Get starting position - distance above player
		var startingPosition = RespawnOnDeath.Instance.DeathPositionWorld;
		startingPosition += RespawnOnDeath.Instance.DeathPlayerUpVector * num5 * __instance._observatoryRevealDist;

		// Lerp to final rotation
		__instance.transform.rotation = Quaternion.Lerp(lookingDownAtPlayer, finalRotation, num5);

		// Lerp reveal twist
		__instance.transform.rotation *= Quaternion.AngleAxis(Mathf.Lerp(__instance._observatoryRevealTwist, 0f, num4), Vector3.forward);

		var endPosition = __instance._position + (-__instance.transform.forward * __instance._zoom) + Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().GetPosition();

		// Lerp to final position
		__instance.transform.position = Vector3.Lerp(startingPosition, endPosition, num5);

		return false;
	}
}