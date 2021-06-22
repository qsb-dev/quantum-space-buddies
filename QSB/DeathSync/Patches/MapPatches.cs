using OWML.Utils;
using QSB.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace QSB.DeathSync.Patches
{
	class MapPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			Prefix(nameof(MapController_LateUpdate));
		}

		public static bool MapController_LateUpdate(
			MapController __instance,
			bool ____isMapMode,
			bool ____isTrailerMap,
			ref bool ____isObservatoryMap,
			ref float ____observatoryRevealTwist,
			ref float ____defaultPitchAngle,
			ref float ____initialPitchAngle,
			ref float ____defaultZoomDist,
			ref float ____observatoryRevealLength,
			OWCamera ____mapCamera,
			ReferenceFrame ____currentRFrame,
			ref float ____lockTimer,
			ref float ____gridTimer,
			ref float ____revealTimer,
			float ____lockOnMoveLength,
			ref bool ____gridOverride,
			float ____gridLockOnLength,
			float ____revealLength,
			float ____observatoryInteractDelay,
			ref bool ____screenPromptsVisible,
			bool ____isPaused,
			ScreenPrompt ____closePrompt,
			ScreenPrompt ____panPrompt,
			ScreenPrompt ____rotatePrompt,
			ScreenPrompt ____zoomPrompt,
			MeshRenderer ____gridRenderer,
			ref bool ____lockedToTargetTransform,
			ref bool ____interpPosition,
			ref bool ____interpPitch,
			ref bool ____interpZoom,
			ref bool ____framingPlayer,
			OWCamera ____activeCam,
			Transform ____targetTransform,
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
			AnimationCurve ____revealCurve,
			float ____observatoryRevealDist,
			float ____gridSize,
			float ____gridOverrideSize,
			Color ____gridColor
			)
		{
			if (!____isMapMode)
			{
				if (OWInput.IsInputMode(InputMode.Character | InputMode.ShipCockpit) && OWInput.IsNewlyPressed(InputLibrary.map, InputMode.All))
				{
					if (PlayerState.InBrambleDimension() || PlayerState.OnQuantumMoon())
					{
						NotificationManager.SharedInstance.PostNotification(new NotificationData(UITextLibrary.GetString(UITextType.NotificationUnableToOpenMap)), false);
					}
					else
					{
						if (____isTrailerMap)
						{
							____isObservatoryMap = true;
							____observatoryRevealTwist = 0f;
							____defaultPitchAngle = 30f;
							____initialPitchAngle = 0f;
							____defaultZoomDist = 35000f;
							____observatoryRevealLength = 20f;
							____mapCamera.fieldOfView = 70f;
						}
						__instance.GetType().GetAnyMethod("EnterMapView").Invoke(__instance, new object[] { (____currentRFrame == null || !(____currentRFrame.GetOWRigidBody() != null)) ? null : ____currentRFrame.GetOWRigidBody().transform });
					}
				}
			}
			else
			{
				____lockTimer = Mathf.Min(____lockTimer + Time.deltaTime, ____lockOnMoveLength);
				var t = Mathf.Clamp01(____lockTimer / ____lockOnMoveLength);
				____gridTimer = Mathf.Clamp((!____gridOverride) ? (____gridTimer - Time.deltaTime) : (____gridTimer + Time.deltaTime), 0f, ____gridLockOnLength);
				var t2 = Mathf.Clamp01(____gridTimer / ____gridLockOnLength);
				____revealTimer = Mathf.Min(____revealTimer + Time.deltaTime, ____revealLength);
				var num = Mathf.Clamp01(____revealTimer / ____revealLength);
				var t3 = Mathf.SmoothStep(0f, 1f, num);
				var flag = !____isObservatoryMap || ____revealTimer > ____observatoryInteractDelay;
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
					____closePrompt.SetVisibility(true);
					____panPrompt.SetVisibility(true);
					____rotatePrompt.SetVisibility(true);
					____zoomPrompt.SetVisibility(true);
					____screenPromptsVisible = true;
					____gridRenderer.enabled = !____isTrailerMap;
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
				____lockedToTargetTransform &= (vector.sqrMagnitude < 0.01f);
				____interpPosition &= (vector.sqrMagnitude < 0.01f);
				____interpPitch &= (Mathf.Abs(vector2.y) < 0.1f);
				____interpZoom &= (Mathf.Abs(num2) < 0.1f);
				____framingPlayer &= (____lockedToTargetTransform && ____interpZoom);
				____gridOverride &= ____lockedToTargetTransform;
				if (____interpPosition)
				{
					var a = ____activeCam.transform.position - Locator.GetCenterOfTheUniverse().GetOffsetPosition();
					var b = Vector3.zero;
					if (____lockedToTargetTransform && ____targetTransform != null)
					{
						b = ____targetTransform.position - Locator.GetCenterOfTheUniverse().GetOffsetPosition();
						b.y = 0f;
					}
					____position = Vector3.Lerp(a, b, t3);
				}
				else if (____lockedToTargetTransform && ____targetTransform != null)
				{
					var vector3 = ____targetTransform.position;
					vector3 -= Locator.GetCenterOfTheUniverse().GetOffsetPosition();
					vector3.y = 0f;
					____position = Vector3.Lerp(____position, vector3, t);
				}
				else
				{
					var normalized = Vector3.Scale(__instance.transform.forward + __instance.transform.up, new Vector3(1f, 0f, 1f)).normalized;
					var a2 = __instance.transform.right * vector.x + normalized * vector.y;
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
					if (____framingPlayer)
					{
						var num3 = Vector3.Distance(____playerTransform.position, ____targetTransform.position);
						var value = num3 / Mathf.Tan(0.017453292f * ____mapCamera.fieldOfView * 0.5f) * 1.33f;
						____targetZoom = Mathf.Clamp(value, ____minZoomDistance, ____maxZoomDistance);
					}
					____zoom = Mathf.Lerp(____initialZoomDist, ____targetZoom, t3);
				}
				else
				{
					____zoom += num2 * ____zoomSpeed * Time.deltaTime;
					____zoom = Mathf.Clamp(____zoom, ____minZoomDistance, ____maxZoomDistance);
				}
				____mapCamera.nearClipPlane = Mathf.Lerp(0.1f, 1f, t3);
				var quaternion = Quaternion.Euler(____pitch, ____yaw, 0f);
				var vector4 = ____position + quaternion * Vector3.back * ____zoom + Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().GetPosition();
				if (____isObservatoryMap)
				{
					var num4 = (!____isTrailerMap) ? (num * (2f - num)) : ____revealCurve.Evaluate(num);
					var num5 = (!____isTrailerMap) ? Mathf.SmoothStep(0f, 1f, num4) : num4;
					var a3 = (!____isTrailerMap) ? Quaternion.LookRotation(-____playerTransform.up, Vector3.up) : ____activeCam.transform.rotation;
					var a4 = ____activeCam.transform.position;
					a4 += ((!____isTrailerMap) ? ____playerTransform.up : (-____activeCam.transform.forward)) * num5 * ____observatoryRevealDist;
					__instance.transform.rotation = Quaternion.Lerp(a3, quaternion, num5);
					__instance.transform.rotation *= Quaternion.AngleAxis(Mathf.Lerp(____observatoryRevealTwist, 0f, num4), Vector3.forward);
					vector4 = ____position + -__instance.transform.forward * ____zoom + Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().GetPosition();
					__instance.transform.position = Vector3.Lerp(a4, vector4, num5);
				}
				else
				{
					__instance.transform.rotation = quaternion;
					__instance.transform.position = vector4;
				}
				var a5 = ____zoom * (____gridSize / 1000f);
				var d = Mathf.Lerp(a5, ____gridOverrideSize, t2);
				____gridRenderer.transform.position = ____position + Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().GetPosition();
				____gridRenderer.transform.rotation = ((____position.sqrMagnitude >= 0.001f) ? Quaternion.LookRotation(____position, Vector3.up) : Quaternion.identity);
				____gridRenderer.transform.localScale = Vector3.one * d;
				____gridRenderer.material.color = ____gridColor;
				____gridRenderer.material.SetMatrix("_GridCenterMatrix", Matrix4x4.TRS(Locator.GetCenterOfTheUniverse().GetOffsetPosition(), Quaternion.identity, Vector3.one).inverse);
				if (OWInput.IsInputMode(InputMode.Map) && (OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.All) || OWInput.IsNewlyPressed(InputLibrary.map, InputMode.All)))
				{
					__instance.GetType().GetAnyMethod("ExitMapView").Invoke(__instance, null);
				}
			}

			return false;
		}
	}
}
