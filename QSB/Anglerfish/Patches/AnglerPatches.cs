using HarmonyLib;
using QSB.Anglerfish.Messages;
using QSB.Anglerfish.WorldObjects;
using QSB.Messaging;
using QSB.Patches;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.Anglerfish.Patches;

public class AnglerPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.GetTargetPosition))]
	public static bool GetTargetPosition(AnglerfishController __instance, out Vector3 __result)
	{
		var qsbAngler = __instance.GetWorldObject<QSBAngler>();

		if (qsbAngler.TargetTransform != null)
		{
			__result = qsbAngler.TargetTransform.position;
			return false;
		}

		__result = __instance._brambleBody.transform.TransformPoint(__instance._localDisturbancePos);

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.OnSectorOccupantRemoved))]
	public static bool OnSectorOccupantRemoved(AnglerfishController __instance,
		SectorDetector sectorDetector)
	{
		var qsbAngler = __instance.GetWorldObject<QSBAngler>();

		if (qsbAngler.TargetTransform != null && sectorDetector.GetAttachedOWRigidbody().transform == qsbAngler.TargetTransform)
		{
			qsbAngler.TargetTransform = null;
			__instance.ChangeState(AnglerfishController.AnglerState.Lurking);
			qsbAngler.SendMessage(new AnglerDataMessage(qsbAngler));
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.UpdateState))]
	public static bool UpdateState(AnglerfishController __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var qsbAngler = __instance.GetWorldObject<QSBAngler>();

		switch (__instance._currentState)
		{
			case AnglerfishController.AnglerState.Investigating:
				if ((__instance._brambleBody.transform.TransformPoint(__instance._localDisturbancePos) - __instance._anglerBody.GetPosition()).sqrMagnitude < __instance._arrivalDistance * __instance._arrivalDistance)
				{
					__instance.ChangeState(AnglerfishController.AnglerState.Lurking);
					qsbAngler.SendMessage(new AnglerDataMessage(qsbAngler));
					return false;
				}

				break;
			case AnglerfishController.AnglerState.Chasing:
				if (qsbAngler.TargetTransform == null)
				{
					__instance.ChangeState(AnglerfishController.AnglerState.Lurking);
					qsbAngler.SendMessage(new AnglerDataMessage(qsbAngler));
					return false;
				}

				if ((qsbAngler.TargetTransform.position - __instance._anglerBody.GetPosition()).sqrMagnitude > __instance._escapeDistance * __instance._escapeDistance)
				{
					qsbAngler.TargetTransform = null;
					__instance.ChangeState(AnglerfishController.AnglerState.Lurking);
					qsbAngler.SendMessage(new AnglerDataMessage(qsbAngler));
					return false;
				}

				break;
			case AnglerfishController.AnglerState.Consuming:
				if (!__instance._consumeComplete)
				{
					if (qsbAngler.TargetTransform == null)
					{
						__instance.ChangeState(AnglerfishController.AnglerState.Lurking);
						qsbAngler.SendMessage(new AnglerDataMessage(qsbAngler));
						return false;
					}

					var num = Time.time - __instance._consumeStartTime;
					if (qsbAngler.TargetTransform.CompareTag("Player") && num > __instance._consumeDeathDelay)
					{
						Locator.GetDeathManager().KillPlayer(DeathType.Digestion);
						__instance._consumeComplete = true;
						return false;
					}

					if (qsbAngler.TargetTransform.CompareTag("Ship"))
					{
						if (num > __instance._consumeShipCrushDelay)
						{
							qsbAngler.TargetTransform.GetComponentInChildren<ShipDamageController>().TriggerSystemFailure();
						}

						if (num > __instance._consumeDeathDelay)
						{
							if (PlayerState.IsInsideShip())
							{
								Locator.GetDeathManager().KillPlayer(DeathType.Digestion);
							}

							__instance._consumeComplete = true;
							return false;
						}
					}
				}
				else
				{
					qsbAngler.TargetTransform = null;
					__instance.ChangeState(AnglerfishController.AnglerState.Lurking);
					qsbAngler.SendMessage(new AnglerDataMessage(qsbAngler));
				}

				break;
			case AnglerfishController.AnglerState.Stunned:
				__instance._stunTimer -= Time.deltaTime;
				if (__instance._stunTimer <= 0f)
				{
					if (qsbAngler.TargetTransform != null)
					{
						__instance.ChangeState(AnglerfishController.AnglerState.Chasing);
						qsbAngler.SendMessage(new AnglerDataMessage(qsbAngler));
						return false;
					}

					__instance.ChangeState(AnglerfishController.AnglerState.Lurking);
					qsbAngler.SendMessage(new AnglerDataMessage(qsbAngler));
				}

				break;
			default:
				return false;
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.UpdateMovement))]
	public static bool UpdateMovement(AnglerfishController __instance)
	{
		if (!QSBWorldSync.AllObjectsReady)
		{
			return true;
		}

		var qsbAngler = __instance.GetWorldObject<QSBAngler>();
		qsbAngler.UpdateTargetVelocity();

		if (__instance._anglerBody.GetVelocity().sqrMagnitude > Mathf.Pow(__instance._chaseSpeed * 1.5f, 2f))
		{
			__instance.ApplyDrag(10f);
		}

		switch (__instance._currentState)
		{
			case AnglerfishController.AnglerState.Lurking:
				__instance.ApplyDrag(1f);
				return false;
			case AnglerfishController.AnglerState.Investigating:
				{
					var targetPos = __instance._brambleBody.transform.TransformPoint(__instance._localDisturbancePos);
					__instance.RotateTowardsTarget(targetPos, __instance._turnSpeed, __instance._turnSpeed);
					if (!__instance._turningInPlace)
					{
						__instance.MoveTowardsTarget(targetPos, __instance._investigateSpeed, __instance._acceleration);
						return false;
					}

					break;
				}
			case AnglerfishController.AnglerState.Chasing:
				{
					var velocity = qsbAngler.TargetVelocity;
					var normalized = velocity.normalized;
					var from = __instance._anglerBody.GetPosition() + __instance.transform.TransformDirection(__instance._mouthOffset) - qsbAngler.TargetTransform.position;
					var magnitude = velocity.magnitude;
					var num = Vector3.Angle(from, normalized);
					var num2 = magnitude * 2f;
					var d = num2;
					if (num < 90f)
					{
						var magnitude2 = from.magnitude;
						var num3 = magnitude2 * Mathf.Sin(num * 0.017453292f);
						var num4 = magnitude2 * Mathf.Cos(num * 0.017453292f);
						var magnitude3 = __instance._anglerBody.GetVelocity().magnitude;
						var num5 = num4 / Mathf.Max(magnitude, 0.0001f);
						var num6 = num3 / Mathf.Max(magnitude3, 0.0001f);
						var num7 = num5 / num6;
						if (num7 <= 1f)
						{
							var t = Mathf.Clamp01(num7);
							d = Mathf.Lerp(num2, num4, t);
						}
						else
						{
							var num8 = Mathf.InverseLerp(1f, 4f, num7);
							d = Mathf.Lerp(num4, 0f, num8 * num8);
						}
					}

					__instance._targetPos = qsbAngler.TargetTransform.position + normalized * d;
					__instance.RotateTowardsTarget(__instance._targetPos, __instance._turnSpeed, __instance._quickTurnSpeed);
					if (!__instance._turningInPlace)
					{
						__instance.MoveTowardsTarget(__instance._targetPos, __instance._chaseSpeed, __instance._acceleration);
						return false;
					}

					break;
				}
			case AnglerfishController.AnglerState.Consuming:
				__instance.ApplyDrag(1f);
				return false;
			case AnglerfishController.AnglerState.Stunned:
				__instance.ApplyDrag(0.5f);
				break;
			default:
				return false;
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.OnImpact))]
	public static bool OnImpact(AnglerfishController __instance,
		ImpactData impact)
	{
		var qsbAngler = __instance.GetWorldObject<QSBAngler>();

		var attachedOWRigidbody = impact.otherCollider.GetAttachedOWRigidbody();
		if ((attachedOWRigidbody.CompareTag("Player") || attachedOWRigidbody.CompareTag("Ship"))
			&& __instance._currentState != AnglerfishController.AnglerState.Consuming
			&& __instance._currentState != AnglerfishController.AnglerState.Stunned)
		{
			qsbAngler.TargetTransform = attachedOWRigidbody.transform;
			__instance.ChangeState(AnglerfishController.AnglerState.Chasing);
			qsbAngler.SendMessage(new AnglerDataMessage(qsbAngler));
			return false;
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.OnClosestAudibleNoise))]
	public static bool OnClosestAudibleNoise(AnglerfishController __instance,
		NoiseMaker noiseMaker)
	{
		var qsbAngler = __instance.GetWorldObject<QSBAngler>();

		if (__instance._currentState is AnglerfishController.AnglerState.Consuming or AnglerfishController.AnglerState.Stunned)
		{
			return false;
		}

		if ((noiseMaker.GetNoiseOrigin() - __instance.transform.position).sqrMagnitude < __instance._pursueDistance * __instance._pursueDistance)
		{
			if (qsbAngler.TargetTransform != noiseMaker.GetAttachedBody().transform)
			{
				qsbAngler.TargetTransform = noiseMaker.GetAttachedBody().transform;
				if (__instance._currentState != AnglerfishController.AnglerState.Chasing)
				{
					__instance.ChangeState(AnglerfishController.AnglerState.Chasing);
				}

				qsbAngler.SendMessage(new AnglerDataMessage(qsbAngler));
				return false;
			}
		}
		else if (__instance._currentState is AnglerfishController.AnglerState.Lurking or AnglerfishController.AnglerState.Investigating)
		{
			__instance._localDisturbancePos = __instance._brambleBody.transform.InverseTransformPoint(noiseMaker.GetNoiseOrigin());
			if (__instance._currentState != AnglerfishController.AnglerState.Investigating)
			{
				__instance.ChangeState(AnglerfishController.AnglerState.Investigating);
			}

			qsbAngler.SendMessage(new AnglerDataMessage(qsbAngler));
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AnglerfishController), nameof(AnglerfishController.OnCaughtObject))]
	public static bool OnCaughtObject(AnglerfishController __instance,
		OWRigidbody caughtBody)
	{
		var qsbAngler = __instance.GetWorldObject<QSBAngler>();

		if (__instance._currentState == AnglerfishController.AnglerState.Consuming)
		{
			if (!qsbAngler.TargetTransform.CompareTag("Player") && caughtBody.CompareTag("Player"))
			{
				Locator.GetDeathManager().KillPlayer(DeathType.Digestion);
			}

			return false;
		}

		if (caughtBody.CompareTag("Player") || caughtBody.CompareTag("Ship"))
		{
			qsbAngler.TargetTransform = caughtBody.transform;
			__instance._consumeStartTime = Time.time;
			__instance.ChangeState(AnglerfishController.AnglerState.Consuming);
			qsbAngler.SendMessage(new AnglerDataMessage(qsbAngler));
		}

		return false;
	}
}