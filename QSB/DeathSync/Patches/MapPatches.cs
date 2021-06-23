using OWML.Utils;
using QSB.Patches;
using QSB.Utility;
using UnityEngine;

namespace QSB.DeathSync.Patches
{
	class MapPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.RespawnTime;

		public override void DoPatches()
		{
			Prefix(nameof(MapController_LateUpdate));
			Prefix(nameof(MapController_EnterMapView));
		}

		public static bool MapController_EnterMapView(
			MapController __instance,
			ref bool ____isMapMode,
			OWAudioSource ____audioSource,
			MapMarkerManager ____mapMarkerManager,
			OWCamera ____mapCamera,
			OWCamera ____activeCam,
			MeshRenderer ____gridRenderer,
			ref Transform ____targetTransform,
			ref bool ____lockedToTargetTransform,
			ref Vector3 ____position,
			ref float ____yaw,
			ref float ____pitch,
			ref float ____zoom,
			ref float ____targetZoom,
			ref bool ____interpPosition,
			ref bool ____interpPitch,
			ref bool ____interpZoom,
			ref bool ____framingPlayer,
			ref float ____lockTimer,
			float ____defaultYawAngle,
			float ____initialPitchAngle,
			float ____initialZoomDist,
			float ____defaultZoomDist,
			float ____lockOnMoveLength,
			ref float ____gridOverrideSize,
			ref bool ____gridOverride,
			ref float ____gridTimer,
			ref float ____revealLength,
			Transform ____playerTransform,
			ReferenceFrame ____currentRFrame,
			float ____gridLockOnLength,
			ref float ____revealTimer,
			float ____observatoryRevealLength
			)
		{
			if (____isMapMode)
			{
				return false;
			}

			____mapMarkerManager.SetVisible(true);
			GlobalMessenger.FireEvent("EnterMapView");
			GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", ____mapCamera);
			if (____audioSource.isPlaying)
			{
				____audioSource.Stop();
				____audioSource.SetLocalVolume(1f);
				____audioSource.Play();
			}
			else
			{
				____audioSource.SetLocalVolume(1f);
				____audioSource.Play();
			}

			Locator.GetAudioMixer().MixMap();
			____activeCam.enabled = false;
			____mapCamera.enabled = true;
			____gridRenderer.enabled = false;
			____targetTransform = null;
			____lockedToTargetTransform = false;
			____position = ____playerTransform.position - Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().GetPosition();
			____position.y = 0f;
			____yaw = ____defaultYawAngle;
			____pitch = ____initialPitchAngle;
			____zoom = ____initialZoomDist;
			____targetZoom = ____defaultZoomDist;
			__instance.transform.rotation = Quaternion.LookRotation(-____playerTransform.up, ____playerTransform.forward);
			__instance.transform.position = ____activeCam.transform.position;
			____interpPosition = true;
			____interpPitch = true;
			____interpZoom = true;
			____framingPlayer = ____lockedToTargetTransform;
			____lockTimer = ____lockOnMoveLength;
			____gridOverrideSize = (____currentRFrame == null) ? 0f : ____currentRFrame.GetAutopilotArrivalDistance();
			____gridOverride = ____gridOverrideSize > 0f;
			____gridTimer = (!____gridOverride) ? 0f : ____gridLockOnLength;
			____revealLength = ____observatoryRevealLength;
			____revealTimer = 0f;
			____isMapMode = true;
			return false;
		}

		public static bool MapController_LateUpdate(
			MapController __instance,
			ref float ____observatoryRevealTwist,
			ref float ____defaultPitchAngle,
			ref float ____initialPitchAngle,
			OWCamera ____mapCamera,
			ref float ____lockTimer,
			ref float ____revealTimer,
			float ____lockOnMoveLength,
			float ____revealLength,
			float ____observatoryInteractDelay,
			ref bool ____screenPromptsVisible,
			bool ____isPaused,
			ScreenPrompt ____closePrompt,
			ScreenPrompt ____panPrompt,
			ScreenPrompt ____rotatePrompt,
			ScreenPrompt ____zoomPrompt,
			ref bool ____lockedToTargetTransform,
			ref bool ____interpPosition,
			ref bool ____interpPitch,
			ref bool ____interpZoom,
			OWCamera ____activeCam,
			ref Vector3 ____position,
			float ____panSpeed,
			ref float ____zoom,
			float ____maxPanDistance,
			float ____yawSpeed,
			ref float ____yaw,
			float ____pitchSpeed,
			ref float ____pitch,
			float ____minPitchAngle,
			float ____maxPitchAngle,
			Transform ____playerTransform,
			ref float ____targetZoom,
			float ____minZoomDistance,
			float ____maxZoomDistance,
			float ____initialZoomDist,
			float ____zoomSpeed,
			float ____observatoryRevealDist
			)
		{
			____lockTimer = Mathf.Min(____lockTimer + Time.deltaTime, ____lockOnMoveLength);
			____revealTimer = Mathf.Min(____revealTimer + Time.deltaTime, ____revealLength);
			var num = Mathf.Clamp01(____revealTimer / ____revealLength);
			var t3 = Mathf.SmoothStep(0f, 1f, num);
			var flag = ____revealTimer > ____observatoryInteractDelay;
			if (____screenPromptsVisible && ____isPaused)
			{
				____closePrompt.SetVisibility(false);
				____panPrompt.SetVisibility(false);
				____rotatePrompt.SetVisibility(false);
				____zoomPrompt.SetVisibility(false);
				____screenPromptsVisible = false;
			}
			else if (!____screenPromptsVisible && flag && !____isPaused)
			{
				____closePrompt.SetVisibility(false);
				____panPrompt.SetVisibility(true);
				____rotatePrompt.SetVisibility(true);
				____zoomPrompt.SetVisibility(true);
				____screenPromptsVisible = true;
			}

			var vector = Vector2.zero;
			var vector2 = Vector2.zero;
			var num2 = 0f;
			if (flag)
			{
				vector = OWInput.GetValue(InputLibrary.moveXZ, InputMode.All);
				vector2 = InputLibrary.look.GetValue(false);
				num2 = OWInput.GetValue(InputLibrary.mapZoom, InputMode.All);
				vector2.y *= -1f;
				num2 *= -1f;
			}

			____lockedToTargetTransform &= vector.sqrMagnitude < 0.01f;
			____interpPosition &= vector.sqrMagnitude < 0.01f;
			____interpPitch &= Mathf.Abs(vector2.y) < 0.1f;
			____interpZoom &= Mathf.Abs(num2) < 0.1f;

			if (____interpPosition)
			{
				var a = ____activeCam.transform.position - Locator.GetCenterOfTheUniverse().GetOffsetPosition();
				var b = Vector3.zero;
				____position = Vector3.Lerp(a, b, t3);
			}
			else
			{
				var normalized = Vector3.Scale(__instance.transform.forward + __instance.transform.up, new Vector3(1f, 0f, 1f)).normalized;
				var a2 = (__instance.transform.right * vector.x) + (normalized * vector.y);
				____position += a2 * ____panSpeed * ____zoom * Time.deltaTime;
				____position.y = 0f;
				if (____position.sqrMagnitude > ____maxPanDistance * ____maxPanDistance)
				{
					____position = ____position.normalized * ____maxPanDistance;
				}
			}

			____yaw += vector2.x * ____yawSpeed * Time.deltaTime;
			____yaw = OWMath.WrapAngle(____yaw);
			if (____interpPitch)
			{
				____pitch = Mathf.Lerp(____initialPitchAngle, ____defaultPitchAngle, t3);
			}
			else
			{
				____pitch += vector2.y * ____pitchSpeed * Time.deltaTime;
				____pitch = Mathf.Clamp(____pitch, ____minPitchAngle, ____maxPitchAngle);
			}

			if (____interpZoom)
			{
				____zoom = Mathf.Lerp(____initialZoomDist, ____targetZoom, t3);
			}
			else
			{
				____zoom += num2 * ____zoomSpeed * Time.deltaTime;
				____zoom = Mathf.Clamp(____zoom, ____minZoomDistance, ____maxZoomDistance);
			}

			____mapCamera.nearClipPlane = Mathf.Lerp(0.1f, 1f, t3);
			var quaternion = Quaternion.Euler(____pitch, ____yaw, 0f);
			var num4 = num * (2f - num);
			var num5 = Mathf.SmoothStep(0f, 1f, num4);
			var a3 = Quaternion.LookRotation(-____playerTransform.up, Vector3.up);
			var a4 = ____activeCam.transform.position;
			a4 += ____playerTransform.up * num5 * ____observatoryRevealDist;
			__instance.transform.rotation = Quaternion.Lerp(a3, quaternion, num5);
			__instance.transform.rotation *= Quaternion.AngleAxis(Mathf.Lerp(____observatoryRevealTwist, 0f, num4), Vector3.forward);
			var vector4 = ____position + (-__instance.transform.forward * ____zoom) + Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().GetPosition();
			__instance.transform.position = Vector3.Lerp(a4, vector4, num5);

			return false;
		}
	}
}
