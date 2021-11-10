using HarmonyLib;
using QSB.Anglerfish.WorldObjects;
using QSB.Events;
using QSB.Patches;
using QSB.WorldSync;
using System;
using UnityEngine;

namespace QSB.Anglerfish.Patches
{
	public class AnglerPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.GetTargetPosition))]
		public static bool GetTargetPosition(AnglerfishController __instance, ref Vector3 __result)
		{
			var qsbAngler = QSBWorldSync.GetWorldFromUnity<QSBAngler>(__instance);
			if (qsbAngler == null || qsbAngler.TransformSync == null)
			{
				return false;
			}

			__result = qsbAngler.TargetTransform != null
				? qsbAngler.TargetTransform.position
				: __instance._brambleBody.transform.TransformPoint(__instance._localDisturbancePos);

			return false;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.OnSectorOccupantRemoved))]
		public static bool OnSectorOccupantRemoved(AnglerfishController __instance,
			SectorDetector sectorDetector)
		{
			var qsbAngler = QSBWorldSync.GetWorldFromUnity<QSBAngler>(__instance);
			if (qsbAngler == null || qsbAngler.TransformSync == null)
			{
				return false;
			}

			if (!(qsbAngler.TargetTransform != null) || !(sectorDetector.GetAttachedOWRigidbody().transform == qsbAngler.TargetTransform))
			{
				return false;
			}

			qsbAngler.TargetTransform = null;

			return false;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.UpdateState))]
		public static bool UpdateState(AnglerfishController __instance)
		{
			var qsbAngler = QSBWorldSync.GetWorldFromUnity<QSBAngler>(__instance);
			if (qsbAngler == null || qsbAngler.TransformSync == null)
			{
				return false;
			}

			switch (__instance._currentState)
			{
				case AnglerfishController.AnglerState.Investigating:
					if ((__instance._brambleBody.transform.TransformPoint(__instance._localDisturbancePos) - __instance._anglerBody.GetPosition()).sqrMagnitude >= __instance._arrivalDistance * (double)__instance._arrivalDistance)
					{
						break;
					}

					__instance.ChangeState(AnglerfishController.AnglerState.Lurking);
					QSBEventManager.FireEvent(EventNames.QSBAnglerChangeState, qsbAngler);
					break;

				case AnglerfishController.AnglerState.Chasing:
					if (qsbAngler.TargetTransform == null)
					{
						__instance.ChangeState(AnglerfishController.AnglerState.Lurking);
						QSBEventManager.FireEvent(EventNames.QSBAnglerChangeState, qsbAngler);
						break;
					}

					if ((qsbAngler.TargetTransform.position - __instance._anglerBody.GetPosition()).sqrMagnitude <= __instance._escapeDistance * (double)__instance._escapeDistance)
					{
						break;
					}

					qsbAngler.TargetTransform = null;
					__instance.ChangeState(AnglerfishController.AnglerState.Lurking);
					QSBEventManager.FireEvent(EventNames.QSBAnglerChangeState, qsbAngler);
					break;

				case AnglerfishController.AnglerState.Consuming:
					if (__instance._consumeComplete)
					{
						break;
					}

					if (qsbAngler.TargetTransform == null)
					{
						__instance.ChangeState(AnglerfishController.AnglerState.Lurking);
						QSBEventManager.FireEvent(EventNames.QSBAnglerChangeState, qsbAngler);
						break;
					}

					var num = Time.time - __instance._consumeStartTime;
					if (qsbAngler.TargetTransform.CompareTag("Player") && num > (double)__instance._consumeDeathDelay)
					{
						qsbAngler.TargetTransform = null;
						__instance.ChangeState(AnglerfishController.AnglerState.Lurking);
						QSBEventManager.FireEvent(EventNames.QSBAnglerChangeState, qsbAngler);

						Locator.GetDeathManager().KillPlayer(DeathType.Digestion);
						__instance._consumeComplete = true;
						break;
					}

					if (!qsbAngler.TargetTransform.CompareTag("Ship"))
					{
						break;
					}

					if (num > (double)__instance._consumeShipCrushDelay)
					{
						qsbAngler.TargetTransform.GetComponentInChildren<ShipDamageController>().TriggerSystemFailure();
					}

					if (num <= (double)__instance._consumeDeathDelay)
					{
						break;
					}

					if (PlayerState.IsInsideShip())
					{
						qsbAngler.TargetTransform = null;
						__instance.ChangeState(AnglerfishController.AnglerState.Lurking);
						QSBEventManager.FireEvent(EventNames.QSBAnglerChangeState, qsbAngler);

						Locator.GetDeathManager().KillPlayer(DeathType.Digestion);
					}

					__instance._consumeComplete = true;
					break;

				case AnglerfishController.AnglerState.Stunned:
					__instance._stunTimer -= Time.deltaTime;
					if (__instance._stunTimer > 0.0)
					{
						break;
					}

					if (qsbAngler.TargetTransform != null)
					{
						__instance.ChangeState(AnglerfishController.AnglerState.Chasing);
						QSBEventManager.FireEvent(EventNames.QSBAnglerChangeState, qsbAngler);
						break;
					}
					__instance.ChangeState(AnglerfishController.AnglerState.Lurking);
					QSBEventManager.FireEvent(EventNames.QSBAnglerChangeState, qsbAngler);
					break;
			}

			return false;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.UpdateMovement))]
		public static bool UpdateMovement(AnglerfishController __instance)
		{
			var qsbAngler = QSBWorldSync.GetWorldFromUnity<QSBAngler>(__instance);
			if (qsbAngler == null || qsbAngler.TransformSync == null)
			{
				return false;
			}

			if (!qsbAngler.TransformSync.HasAuthority)
			{
				return false;
			}

			qsbAngler.FixedUpdate();

			if (__instance._anglerBody.GetVelocity().sqrMagnitude > (double)Mathf.Pow(__instance._chaseSpeed * 1.5f, 2f))
			{
				__instance.ApplyDrag(10f);
			}

			switch (__instance._currentState)
			{
				case AnglerfishController.AnglerState.Lurking:
					__instance.ApplyDrag(1f);
					break;

				case AnglerfishController.AnglerState.Investigating:
					var targetPos = __instance._brambleBody.transform.TransformPoint(__instance._localDisturbancePos);
					__instance.RotateTowardsTarget(targetPos, __instance._turnSpeed, __instance._turnSpeed);
					if (__instance._turningInPlace)
					{
						break;
					}

					__instance.MoveTowardsTarget(targetPos, __instance._investigateSpeed, __instance._acceleration);
					break;

				case AnglerfishController.AnglerState.Chasing:
					var velocity = qsbAngler.TargetVelocity;
					var normalized = velocity.normalized;
					var from = __instance._anglerBody.GetPosition() + __instance.transform.TransformDirection(__instance._mouthOffset) - qsbAngler.TargetTransform.position;
					var magnitude1 = velocity.magnitude;
					var num1 = Vector3.Angle(from, normalized);
					var a = magnitude1 * 2f;
					var num2 = a;
					if (num1 < 90.0)
					{
						var magnitude2 = (double)from.magnitude;
						var num3 = (float)magnitude2 * Mathf.Sin(num1 * ((float)Math.PI / 180f));
						var num4 = (float)magnitude2 * Mathf.Cos(num1 * ((float)Math.PI / 180f));
						var magnitude3 = __instance._anglerBody.GetVelocity().magnitude;
						var num5 = num4 / Mathf.Max(magnitude1, 0.0001f) / (num3 / Mathf.Max(magnitude3, 0.0001f));
						if (num5 <= 1.0)
						{
							var t = Mathf.Clamp01(num5);
							num2 = Mathf.Lerp(a, num4, t);
						}
						else
						{
							var num6 = Mathf.InverseLerp(1f, 4f, num5);
							num2 = Mathf.Lerp(num4, 0.0f, num6 * num6);
						}
					}

					__instance._targetPos = qsbAngler.TargetTransform.position + normalized * num2;
					__instance.RotateTowardsTarget(__instance._targetPos, __instance._turnSpeed, __instance._quickTurnSpeed);
					if (__instance._turningInPlace)
					{
						break;
					}

					__instance.MoveTowardsTarget(__instance._targetPos, __instance._chaseSpeed, __instance._acceleration);
					break;

				case AnglerfishController.AnglerState.Consuming:
					__instance.ApplyDrag(1f);
					break;

				case AnglerfishController.AnglerState.Stunned:
					__instance.ApplyDrag(0.5f);
					break;
			}

			return false;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.OnImpact))]
		public static bool OnImpact(AnglerfishController __instance,
			ImpactData impact)
		{
			var qsbAngler = QSBWorldSync.GetWorldFromUnity<QSBAngler>(__instance);
			if (qsbAngler == null || qsbAngler.TransformSync == null)
			{
				return false;
			}

			var attachedOwRigidbody = impact.otherCollider.GetAttachedOWRigidbody();
			if ((attachedOwRigidbody.CompareTag("Player") || attachedOwRigidbody.CompareTag("Ship"))
				&& __instance._currentState != AnglerfishController.AnglerState.Consuming
				&& __instance._currentState != AnglerfishController.AnglerState.Stunned)
			{
				qsbAngler.TargetTransform = attachedOwRigidbody.transform;
				__instance.ChangeState(AnglerfishController.AnglerState.Chasing);
				QSBEventManager.FireEvent(EventNames.QSBAnglerChangeState, qsbAngler);
			}

			return false;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.OnClosestAudibleNoise))]
		public static bool OnClosestAudibleNoise(AnglerfishController __instance,
			NoiseMaker noiseMaker)
		{
			var qsbAngler = QSBWorldSync.GetWorldFromUnity<QSBAngler>(__instance);
			if (qsbAngler == null || qsbAngler.TransformSync == null)
			{
				return false;
			}

			if (__instance._currentState == AnglerfishController.AnglerState.Consuming
				|| __instance._currentState == AnglerfishController.AnglerState.Stunned)
			{
				return false;
			}

			if ((noiseMaker.GetNoiseOrigin() - __instance.transform.position).sqrMagnitude < __instance._pursueDistance * (double)__instance._pursueDistance)
			{
				if (!(qsbAngler.TargetTransform != noiseMaker.GetAttachedBody().transform))
				{
					return false;
				}

				qsbAngler.TargetTransform = noiseMaker.GetAttachedBody().transform;
				if (__instance._currentState == AnglerfishController.AnglerState.Chasing)
				{
					return false;
				}

				__instance.ChangeState(AnglerfishController.AnglerState.Chasing);
				QSBEventManager.FireEvent(EventNames.QSBAnglerChangeState, qsbAngler);
			}
			else
			{
				if (__instance._currentState != AnglerfishController.AnglerState.Lurking
					&& __instance._currentState != AnglerfishController.AnglerState.Investigating)
				{
					return false;
				}

				__instance._localDisturbancePos = __instance._brambleBody.transform.InverseTransformPoint(noiseMaker.GetNoiseOrigin());
				if (__instance._currentState == AnglerfishController.AnglerState.Investigating)
				{
					return false;
				}

				__instance.ChangeState(AnglerfishController.AnglerState.Investigating);
				QSBEventManager.FireEvent(EventNames.QSBAnglerChangeState, qsbAngler);
			}

			return false;
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.OnCaughtObject))]
		public static bool OnCaughtObject(AnglerfishController __instance,
			OWRigidbody caughtBody)
		{
			var qsbAngler = QSBWorldSync.GetWorldFromUnity<QSBAngler>(__instance);

			if (__instance._currentState == AnglerfishController.AnglerState.Consuming)
			{
				if (qsbAngler.TargetTransform.CompareTag("Player") || !caughtBody.CompareTag("Player"))
				{
					return false;
				}

				Locator.GetDeathManager().KillPlayer(DeathType.Digestion);
			}
			else
			{
				if (!caughtBody.CompareTag("Player") && !caughtBody.CompareTag("Ship"))
				{
					return false;
				}

				qsbAngler.TargetTransform = caughtBody.transform;
				__instance._consumeStartTime = Time.time;
				__instance.ChangeState(AnglerfishController.AnglerState.Consuming);
				QSBEventManager.FireEvent(EventNames.QSBAnglerChangeState, qsbAngler);
			}

			return false;
		}
	}
}
