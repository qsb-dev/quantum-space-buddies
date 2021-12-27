using HarmonyLib;
using OWML.Utils;
using QSB.Messaging;
using QSB.Patches;
using QSB.ShipSync.Messages;
using QSB.ShipSync.Messages.Component;
using QSB.ShipSync.Messages.Hull;
using QSB.ShipSync.TransformSync;
using QSB.ShipSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using UnityEngine;

namespace QSB.ShipSync.Patches
{
	[HarmonyPatch]
	internal class ShipPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(HatchController), nameof(HatchController.OnPressInteract))]
		public static bool HatchController_OnPressInteract()
		{
			if (!PlayerState.IsInsideShip())
			{
				ShipManager.Instance.ShipTractorBeam.ActivateTractorBeam();
				new FunnelEnableMessage().Send();
			}

			new HatchMessage(true).Send();
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(HatchController), nameof(HatchController.OnEntry))]
		public static bool HatchController_OnEntry(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				new HatchMessage(false).Send();
			}

			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ShipTractorBeamSwitch), nameof(ShipTractorBeamSwitch.OnTriggerExit))]
		public static bool ShipTractorBeamSwitch_OnTriggerExit(ShipTractorBeamSwitch __instance, Collider hitCollider)
		{
			if (!__instance._isPlayerInShip && __instance._functional && hitCollider.CompareTag("PlayerDetector") && !ShipManager.Instance.HatchController._hatchObject.activeSelf)
			{
				ShipManager.Instance.HatchController.Invoke("CloseHatch");
				ShipManager.Instance.ShipTractorBeam.DeactivateTractorBeam();
				new HatchMessage(false).Send();
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(InteractZone), nameof(InteractZone.UpdateInteractVolume))]
		public static bool InteractZone_UpdateInteractVolume(InteractZone __instance)
		{
			/* Angle for interaction with the ship hatch
			 *
			 *  \  80°  / - If in ship
			 *   \     /
			 *    \   /
			 *   [=====]  - Hatch
			 *    /   \
			 *   /     \
			 *  / 280°  \ - If not in ship
			 *
			 */

			if (!WorldObjectManager.AllObjectsReady || __instance != ShipManager.Instance.HatchInteractZone)
			{
				return true;
			}

			var angle = 2f * Vector3.Angle(__instance._playerCam.transform.forward, __instance.transform.forward);

			__instance._focused = PlayerState.IsInsideShip()
				? angle <= 80
				: angle >= 280;

			__instance.InvokeBase(nameof(InteractZone.UpdateInteractVolume));

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ShipElectricalComponent), nameof(ShipElectricalComponent.OnEnterShip))]
		public static bool ShipElectricalComponent_OnEnterShip(ShipElectricalComponent __instance)
		{
			__instance.InvokeBase(nameof(ShipElectricalComponent.OnEnterShip));

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ShipElectricalComponent), nameof(ShipElectricalComponent.OnExitShip))]
		public static bool ShipElectricalComponent_OnExitShip(ShipElectricalComponent __instance)
		{
			__instance.InvokeBase(nameof(ShipElectricalComponent.OnExitShip));

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ShipComponent), nameof(ShipComponent.SetDamaged))]
		public static bool ShipComponent_SetDamaged(ShipComponent __instance, bool damaged)
		{
			if (__instance._damaged == damaged)
			{
				return false;
			}

			var qsbShipComponent = __instance.GetWorldObject<QSBShipComponent>();
			if (damaged)
			{
				__instance._damaged = true;
				__instance._repairFraction = 0f;
				__instance.OnComponentDamaged();
				__instance.RaiseEvent(nameof(__instance.OnDamaged), __instance);
				qsbShipComponent
					.SendMessage(new ComponentDamagedMessage());
			}
			else
			{
				__instance._damaged = false;
				__instance._repairFraction = 1f;
				__instance.OnComponentRepaired();
				__instance.RaiseEvent(nameof(__instance.OnRepaired), __instance);
				qsbShipComponent
					.SendMessage(new ComponentRepairedMessage());
			}

			__instance.UpdateColliderState();
			if (__instance._damageEffect)
			{
				__instance._damageEffect.SetEffectBlend(1f - __instance._repairFraction);
			}

			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ShipHull), nameof(ShipHull.FixedUpdate))]
		public static bool ShipHull_FixedUpdate(ShipHull __instance, ref ImpactData ____dominantImpact, ref float ____integrity, ref bool ____damaged, DamageEffect ____damageEffect, ShipComponent[] ____components)
		{
			if (____dominantImpact != null)
			{
				var damage = Mathf.InverseLerp(30f, 200f, ____dominantImpact.speed);
				if (damage > 0f)
				{
					var num2 = 0.15f;
					if (damage < num2 && ____integrity > 1f - num2)
					{
						damage = num2;
					}

					____integrity = Mathf.Max(____integrity - damage, 0f);
					var qsbShipHull = __instance.GetWorldObject<QSBShipHull>();
					if (!____damaged)
					{
						____damaged = true;
						__instance.RaiseEvent(nameof(__instance.OnDamaged), __instance);

						qsbShipHull
							.SendMessage(new HullDamagedMessage());
					}

					if (____damageEffect != null)
					{
						____damageEffect.SetEffectBlend(1f - ____integrity);
					}

					qsbShipHull
						.SendMessage(new HullChangeIntegrityMessage(____integrity));
				}

				foreach (var component in ____components)
				{
					if (!(component == null) && !component.isDamaged)
					{
						if (component.ApplyImpact(____dominantImpact))
						{
							break;
						}
					}
				}

				__instance.RaiseEvent(nameof(__instance.OnImpact), ____dominantImpact, damage);

				____dominantImpact = null;
			}

			__instance.enabled = false;
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ShipDamageController), nameof(ShipDamageController.OnImpact))]
		public static bool ShipDamageController_OnImpact()
			=> ShipTransformSync.LocalInstance == null || ShipManager.Instance.HasAuthority;

		[HarmonyPostfix]
		[HarmonyPatch(typeof(ShipComponent), nameof(ShipComponent.RepairTick))]
		public static void ShipComponent_RepairTick(ShipComponent __instance) =>
			__instance.GetWorldObject<QSBShipComponent>()
				.SendMessage(new ComponentRepairTickMessage(__instance._repairFraction));

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ShipHull), nameof(ShipHull.RepairTick))]
		public static bool ShipHull_RepairTick(ShipHull __instance, ref float ____integrity, ref bool ____damaged, DamageEffect ____damageEffect, float ____repairTime)
		{
			if (!____damaged)
			{
				return false;
			}

			____integrity = Mathf.Min(____integrity + (Time.deltaTime / ____repairTime), 1f);
			var qsbShipHull = __instance.GetWorldObject<QSBShipHull>();
			qsbShipHull
				.SendMessage(new HullRepairTickMessage(____integrity));

			if (____integrity >= 1f)
			{
				____damaged = false;
				__instance.RaiseEvent(nameof(__instance.OnRepaired), __instance);
				qsbShipHull
					.SendMessage(new HullRepairedMessage());
			}

			if (____damageEffect != null)
			{
				____damageEffect.SetEffectBlend(1f - ____integrity);
			}

			return false;
		}
	}
}