using HarmonyLib;
using QSB.Patches;
using QSB.Player;
using QSB.Utility;
using System.Linq;
using UnityEngine;

namespace QSB.EyeOfTheUniverse.MaskSync.Patches;

public class MaskPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	[HarmonyPrefix]
	[HarmonyPatch(typeof(EyeShuttleController), nameof(EyeShuttleController.OnLaunchSlotActivated))]
	public static bool DontLaunch(EyeShuttleController __instance)
	{
		QSBPlayerManager.PlayerList.Where(x => x.IsInEyeShuttle).ForEach(x =>
		{
			MaskManager.WentOnSolanumsWildRide.Add(x);
			x.OnSolanumsWildRide = true;
		});

		if (__instance._isPlayerInside)
		{
			return true;
		}

		MaskManager.FlickerOutShuttle();
		__instance._hasLaunched = true;
		__instance._hasArrivedAtMask = true;
		__instance._hasPlayedOneShot = true;
		__instance.enabled = false;

		return false;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(MaskZoneController), nameof(MaskZoneController.OnFinishGather))]
	public static bool FinishGather(MaskZoneController __instance)
	{
		__instance._shuttle.OnFinishGather();

		if (MaskManager.WentOnSolanumsWildRide.Contains(QSBPlayerManager.LocalPlayer))
		{
			Locator.GetPlayerBody().SetPosition(__instance._returnSocket.position);
			Locator.GetPlayerBody().SetRotation(__instance._returnSocket.rotation);
			Locator.GetPlayerBody().SetVelocity(Vector3.zero);
			var component = Locator.GetPlayerCamera().GetComponent<PlayerCameraController>();
			component.SetDegreesY(component.GetMinDegreesY());
		}

		foreach (var item in MaskManager.WentOnSolanumsWildRide)
		{
			item.OnSolanumsWildRide = false;
		}

		__instance.enabled = false;

		return false;
	}
}