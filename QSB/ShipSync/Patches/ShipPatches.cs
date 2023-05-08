using HarmonyLib;
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

namespace QSB.ShipSync.Patches;

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
			ShipManager.Instance.HatchController.CloseHatch();
			ShipManager.Instance.ShipTractorBeam.DeactivateTractorBeam();
			new HatchMessage(false).Send();
		}

		return false;
	}

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(SingleInteractionVolume), nameof(SingleInteractionVolume.UpdateInteractVolume))]
	public static void SingleInteractionVolume_UpdateInteractVolume_Stub(object instance) { }

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

		if (!QSBWorldSync.AllObjectsReady || __instance != ShipManager.Instance.HatchInteractZone)
		{
			return true;
		}

		var angle = 2f * Vector3.Angle(__instance._playerCam.transform.forward, __instance.transform.forward);

		__instance._focused = PlayerState.IsInsideShip()
			? angle <= 80
			: angle >= 280;

		SingleInteractionVolume_UpdateInteractVolume_Stub(__instance);

		return false;
	}

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(ShipComponent), nameof(ShipComponent.OnEnterShip))]
	public static void ShipComponent_OnEnterShip_Stub(object instance) { }

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ShipElectricalComponent), nameof(ShipElectricalComponent.OnEnterShip))]
	public static bool ShipElectricalComponent_OnEnterShip(ShipElectricalComponent __instance)
	{
		ShipComponent_OnEnterShip_Stub(__instance);

		return false;
	}

	[HarmonyReversePatch]
	[HarmonyPatch(typeof(ShipComponent), nameof(ShipComponent.OnExitShip))]
	public static void ShipComponent_OnExitShip_Stub(object instance) { }

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ShipElectricalComponent), nameof(ShipElectricalComponent.OnExitShip))]
	public static bool ShipElectricalComponent_OnExitShip(ShipElectricalComponent __instance)
	{
		ShipComponent_OnExitShip_Stub(__instance);

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
	public static bool ShipHull_FixedUpdate(ShipHull __instance)
	{
		if (__instance._dominantImpact != null)
		{
			var damage = Mathf.InverseLerp(30f, 200f, __instance._dominantImpact.speed);
			if (damage > 0f)
			{
				var num2 = 0.15f;
				if (damage < num2 && __instance._integrity > 1f - num2)
				{
					damage = num2;
				}

				__instance._integrity = Mathf.Max(__instance._integrity - damage, 0f);
				var qsbShipHull = __instance.GetWorldObject<QSBShipHull>();
				if (!__instance._damaged)
				{
					__instance._damaged = true;
					__instance.RaiseEvent(nameof(__instance.OnDamaged), __instance);

					qsbShipHull
						.SendMessage(new HullDamagedMessage());
				}

				if (__instance._damageEffect != null)
				{
					__instance._damageEffect.SetEffectBlend(1f - __instance._integrity);
				}

				qsbShipHull
					.SendMessage(new HullChangeIntegrityMessage(__instance._integrity));
			}

			foreach (var component in __instance._components)
			{
				if (!(component == null) && !component.isDamaged)
				{
					if (component.ApplyImpact(__instance._dominantImpact))
					{
						break;
					}
				}
			}

			__instance.RaiseEvent(nameof(__instance.OnImpact), __instance._dominantImpact, damage);

			__instance._dominantImpact = null;
		}

		__instance.enabled = false;
		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ShipDamageController), nameof(ShipDamageController.OnImpact))]
	public static bool ShipDamageController_OnImpact()
		=> ShipTransformSync.LocalInstance == null || ShipTransformSync.LocalInstance.isOwned;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ShipComponent), nameof(ShipComponent.RepairTick))]
	public static void ShipComponent_RepairTick(ShipComponent __instance) =>
		__instance.GetWorldObject<QSBShipComponent>()
			.SendMessage(new ComponentRepairTickMessage(__instance._repairFraction));

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ShipHull), nameof(ShipHull.RepairTick))]
	public static bool ShipHull_RepairTick(ShipHull __instance)
	{
		if (!__instance._damaged)
		{
			return false;
		}

		__instance._integrity = Mathf.Min(__instance._integrity + (Time.deltaTime / __instance._repairTime), 1f);
		var qsbShipHull = __instance.GetWorldObject<QSBShipHull>();
		qsbShipHull
			.SendMessage(new HullChangeIntegrityMessage(__instance._integrity));

		if (__instance._integrity >= 1f)
		{
			__instance._damaged = false;
			__instance.RaiseEvent(nameof(__instance.OnRepaired), __instance);
			qsbShipHull
				.SendMessage(new HullRepairedMessage());
		}

		if (__instance._damageEffect != null)
		{
			__instance._damageEffect.SetEffectBlend(1f - __instance._integrity);
		}

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ShipCockpitController), nameof(ShipCockpitController.EnterLandingView))]
	public static void EnterLandingView() =>
		new LandingCameraMessage(true).Send();

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ShipCockpitController), nameof(ShipCockpitController.ExitLandingView))]
	public static void ExitLandingView() =>
		new LandingCameraMessage(false).Send();

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ShipLight), nameof(ShipLight.SetOn))]
	public static void SetOn(ShipLight __instance, bool on)
	{
		if (Remote)
		{
			return;
		}

		if (__instance._on == on)
		{
			return;
		}

		if (!QSBWorldSync.AllObjectsReady)
		{
			return;
		}

		if (!__instance.TryGetWorldObject(out QSBShipLight qsbShipLight))
		{
			return;
		}

		qsbShipLight.SendMessage(new ShipLightMessage(on));
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ShipCockpitUI), nameof(ShipCockpitUI.UpdateSignalscopeCanvas))]
	public static bool UpdateSignalscopeCanvas(ShipCockpitUI __instance)
	{
		var flag = false;
		if (Locator.GetToolModeSwapper().GetToolMode() != ToolMode.SignalScope)
		{
			if (__instance._signalscopeUI.IsActivated())
			{
				__instance._signalscopeUI.Deactivate();
			}
		}
		else if (ShipManager.Instance.CurrentFlyer != uint.MaxValue)
		{
			flag = true;
			if (!__instance._signalscopeUI.IsActivated())
			{
				if (__instance._reticuleController == null)
				{
					Debug.LogError("ReticuleController cannot be null!");
				}

				__instance._signalscopeUI.Activate(__instance._signalscopeTool, __instance._reticuleController);
			}
		}

		__instance._scopeScreenMaterial.SetColor(__instance._propID_EmissionColor, __instance._scopeScreenColor * (flag ? 1f : 0f));
		__instance._sigScopeScreenLight.SetOn(flag);

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ShipReactorComponent), nameof(ShipReactorComponent.OnComponentDamaged))]
	public static bool ByeByeReactor(ShipReactorComponent __instance)
	{
		if (!QSBCore.IsHost)
		{
			return false;
		}

		__instance._criticalCountdown = UnityEngine.Random.Range(__instance._minCountdown, __instance._maxCountdown);
		__instance._criticalTimer = __instance._criticalCountdown;
		__instance.enabled = true;
		new ReactorCountdownMessage(__instance._criticalCountdown).Send();

		return false;
	}
}