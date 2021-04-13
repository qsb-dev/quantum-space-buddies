using OWML.Utils;
using QSB.Events;
using QSB.Patches;
using System.Linq;
using UnityEngine;

namespace QSB.ShipSync.Patches
{
	class ShipPatches : QSBPatch
	{
		public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

		public override void DoPatches()
		{
			QSBCore.HarmonyHelper.AddPrefix<HatchController>("OnPressInteract", typeof(ShipPatches), nameof(HatchController_OnPressInteract));
			QSBCore.HarmonyHelper.AddPrefix<HatchController>("OnEntry", typeof(ShipPatches), nameof(HatchController_OnEntry));
			QSBCore.HarmonyHelper.AddPrefix<ShipTractorBeamSwitch>("OnTriggerExit", typeof(ShipPatches), nameof(ShipTractorBeamSwitch_OnTriggerExit));
		}

		public override void DoUnpatches()
		{
			QSBCore.HarmonyHelper.Unpatch<HatchController>("OnPressInteract");
			QSBCore.HarmonyHelper.Unpatch<HatchController>("OnEntry");
			QSBCore.HarmonyHelper.Unpatch<ShipTractorBeamSwitch>("OnTriggerExit");
		}

		public static bool HatchController_OnPressInteract()
		{
			if (!PlayerState.IsInsideShip())
			{
				ShipManager.Instance.ShipTractorBeam.ActivateTractorBeam();
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
	}
}
