using OWML.Utils;
using QSB.Events;
using QSB.Patches;
using QSB.Utility;
using UnityEngine;

namespace QSB.ShipSync.Patches
{
	internal class ShipPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			Prefix(nameof(HatchController_OnPressInteract));
			Prefix(nameof(HatchController_OnEntry));
			Prefix(nameof(ShipTractorBeamSwitch_OnTriggerExit));
			Prefix(nameof(InteractZone_UpdateInteractVolume));
			Prefix(nameof(ShipElectricalComponent_OnEnterShip));
			Prefix(nameof(ShipElectricalComponent_OnExitShip));
			Prefix(nameof(ElectricalSystem_SetPowered));
			Prefix(nameof(ElectricalComponent_SetPowered));
		}

		public static bool HatchController_OnPressInteract()
		{
			if (!PlayerState.IsInsideShip())
			{
				ShipManager.Instance.ShipTractorBeam.ActivateTractorBeam();
				QSBEventManager.FireEvent(EventNames.QSBEnableFunnel);
			}

			QSBEventManager.FireEvent(EventNames.QSBHatchState, true);
			return true;
		}

		public static bool HatchController_OnEntry(GameObject hitObj)
		{
			if (hitObj.CompareTag("PlayerDetector"))
			{
				QSBEventManager.FireEvent(EventNames.QSBHatchState, false);
			}

			return true;
		}

		public static bool ShipTractorBeamSwitch_OnTriggerExit(Collider hitCollider, bool ____isPlayerInShip, bool ____functional)
		{
			if (!____isPlayerInShip && ____functional && hitCollider.CompareTag("PlayerDetector") && !ShipManager.Instance.HatchController.GetValue<GameObject>("_hatchObject").activeSelf)
			{
				ShipManager.Instance.HatchController.Invoke("CloseHatch");
				ShipManager.Instance.ShipTractorBeam.DeactivateTractorBeam();
				QSBEventManager.FireEvent(EventNames.QSBHatchState, false);
			}

			return false;
		}

		public static bool InteractZone_UpdateInteractVolume(InteractZone __instance, OWCamera ____playerCam, ref bool ____focused)
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

			if (!QSBCore.WorldObjectsReady || __instance != ShipManager.Instance.HatchInteractZone)
			{
				return true;
			}

			var angle = 2f * Vector3.Angle(____playerCam.transform.forward, __instance.transform.forward);

			____focused = PlayerState.IsInsideShip()
				? angle <= 80
				: angle >= 280;

			__instance.CallBase<InteractZone, SingleInteractionVolume>("UpdateInteractVolume");

			return false;
		}

		public static bool ShipElectricalComponent_OnEnterShip(ShipElectricalComponent __instance, bool ____damaged, ElectricalSystem ____electricalSystem)
		{
			__instance.CallBase<ShipElectricalComponent, ShipComponent>("OnEnterShip");
			if (!____damaged)
			{
				DebugLog.DebugWrite($"ShipElectricalComponent - OnEnterShip");
				____electricalSystem.SetPowered(true);
			}

			return false;
		}

		public static bool ShipElectricalComponent_OnExitShip(ShipElectricalComponent __instance, bool ____damaged, ElectricalSystem ____electricalSystem)
		{
			__instance.CallBase<ShipElectricalComponent, ShipComponent>("OnExitShip");
			if (!____damaged)
			{
				DebugLog.DebugWrite($"ShipElectricalComponent - OnExitShip");
				____electricalSystem.SetPowered(false);
			}

			return false;
		}

		public static bool ElectricalSystem_SetPowered(ElectricalSystem __instance, bool powered)
		{
			DebugLog.DebugWrite($"[SYSTEM] {__instance.name} set powered {powered}");
			return true;
		}

		public static bool ElectricalComponent_SetPowered(ElectricalComponent __instance, bool powered)
		{
			DebugLog.DebugWrite($"[COMPONENT] {__instance.name} set powered {powered}");
			return true;
		}
	}
}
