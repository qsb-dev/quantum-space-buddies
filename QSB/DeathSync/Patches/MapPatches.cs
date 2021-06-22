using OWML.Utils;
using QSB.Patches;
using UnityEngine;

namespace QSB.DeathSync.Patches
{
	class MapPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			Prefix(nameof(MapController_LateUpdate));
			Prefix(nameof(MapController_EnterMapView));
		}

		public static bool MapController_EnterMapView(
			MapController __instance,
			Transform targetTransform,
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
			float ____minZoomDistance,
			float ____maxZoomDistance,
			float ____playerFramingScale,
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
			____targetTransform = targetTransform;
			____lockedToTargetTransform = ____targetTransform != null;
			____position = ____playerTransform.position - Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().GetPosition();
			____position.y = 0f;
			____yaw = ____defaultYawAngle;
			____pitch = ____initialPitchAngle;
			____zoom = ____initialZoomDist;
			____targetZoom = ____defaultZoomDist;
			if (____lockedToTargetTransform)
			{
				var num = Vector3.Distance(____playerTransform.position, ____targetTransform.position);
				var value = num / Mathf.Tan(0.017453292f * ____mapCamera.fieldOfView * 0.5f) * ____playerFramingScale;
				____targetZoom = Mathf.Clamp(value, ____minZoomDistance, ____maxZoomDistance);
			}

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
			bool ____isMapMode,
			ref float ____observatoryRevealTwist,
			ref float ____defaultPitchAngle,
			ref float ____initialPitchAngle,
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
					____closePrompt.SetVisibility(true);
					____panPrompt.SetVisibility(true);
					____rotatePrompt.SetVisibility(true);
					____zoomPrompt.SetVisibility(true);
					____screenPromptsVisible = true;
					____gridRenderer.enabled = false;
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
				____framingPlayer &= ____lockedToTargetTransform && ____interpZoom;
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
				var num4 = num * (2f - num);
				var num5 = Mathf.SmoothStep(0f, 1f, num4);
				var a3 = Quaternion.LookRotation(-____playerTransform.up, Vector3.up);
				var a4 = ____activeCam.transform.position;
				a4 += ____playerTransform.up * num5 * ____observatoryRevealDist;
				__instance.transform.rotation = Quaternion.Lerp(a3, quaternion, num5);
				__instance.transform.rotation *= Quaternion.AngleAxis(Mathf.Lerp(____observatoryRevealTwist, 0f, num4), Vector3.forward);
				var vector4 = ____position + (-__instance.transform.forward * ____zoom) + Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().GetPosition();
				__instance.transform.position = Vector3.Lerp(a4, vector4, num5);

				var a5 = ____zoom * (____gridSize / 1000f);
				var d = Mathf.Lerp(a5, ____gridOverrideSize, t2);
				____gridRenderer.transform.position = ____position + Locator.GetCenterOfTheUniverse().GetStaticReferenceFrame().GetPosition();
				____gridRenderer.transform.rotation = (____position.sqrMagnitude >= 0.001f) ? Quaternion.LookRotation(____position, Vector3.up) : Quaternion.identity;
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
