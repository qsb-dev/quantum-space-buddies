using HarmonyLib;
using QSB.Patches;
using QSB.Utility;
using UnityEngine;

namespace QSB.ElevatorSync.Patches;

[HarmonyPatch]
public class NomaiElevatorPatches : QSBPatch
{
	public override QSBPatchTypes Type => QSBPatchTypes.OnClientConnect;

	private static NomaiElevator blackHoleForge;
	private static OWTriggerVolume blackHoleForgeTrigger;
	private static NomaiElevator blackHoleForgeEntrance;
	private static OWTriggerVolume blackHoleForgeEntranceTrigger;

	private static bool runOnce;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(NomaiElevator), nameof(NomaiElevator.FixedUpdate))]
	public static void NomaiElevator_FixedUpdate(NomaiElevator __instance)
	{
		// The forge will handle everything.
		if (__instance.name == "BlackHoleForge_EntrancePivot") return;

		if (!blackHoleForge || !blackHoleForgeTrigger)
		{
			blackHoleForge = GameObject.Find("BlackHoleForgePivot").GetComponent<NomaiElevator>();
			// Use a built-in trigger.
			blackHoleForgeTrigger = GameObject.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/" +
				"Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_BlackHoleForge/BlackHoleForgePivot/" +
				"Volumes_BlackHoleForge/DirectionalForceVolume")
				.GetComponent<OWTriggerVolume>();
		}

		if (!blackHoleForgeEntrance || !blackHoleForgeEntranceTrigger)
		{
			blackHoleForgeEntrance = GameObject.Find("BlackHoleForge_EntrancePivot").GetComponent<NomaiElevator>();
			blackHoleForgeEntranceTrigger = GameObject.Find("FakeSector_BlackHoleForge_EntrancePivot")
				.GetComponent<OWTriggerVolume>();
		}

		var speed = blackHoleForge._speed;
		var player = Locator.GetPlayerDetector();

		var isInForge = blackHoleForgeTrigger.IsTrackingObject(player);
		var isInEntrance = blackHoleForgeEntranceTrigger.IsTrackingObject(player);

		if (isInEntrance)
		{
			// Speed is added to make sure the player moves with the forge AND the entrance.
			speed += blackHoleForgeEntrance._speed;
		}

		if (isInEntrance || isInForge)
		{
			// Players do not move with the forge in the game,
			// so they remain in place while the forge slides past them.
			// This makes sure they move with the forge.
			var newPos = Locator.GetPlayerTransform().position + new Vector3(0f, speed * Time.deltaTime, 0f);
			Locator.GetPlayerTransform().position = newPos;
		}

		if (!runOnce)
		{
			// Recenter the universe because the player has been manually moved.
			runOnce = true;
			
			Delay.RunWhen(() => !__instance.enabled, () =>
			{
				runOnce = false;
				CenterOfTheUniverse.s_instance.OnPlayerRepositioned();
			});
		}
	}
}
