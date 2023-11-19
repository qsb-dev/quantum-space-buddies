using HarmonyLib;
using QSB.Patches;
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

			// Use a built-in trigger and alter it.
			var baseTrigger = GameObject.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/" +
				"Sector_NorthPole/Sector_HangingCity/Sector_HangingCity_BlackHoleForge/" +
				"BlackHoleForgePivot/Interactables_BlackHoleForge/BlackHoleForge_EntrancePivot/" +
				"Geometry_BlackHoleForge_Entrance/GravityVolume (20)");

			var newTrigger = Object.Instantiate(baseTrigger, baseTrigger.transform.parent);
			blackHoleForgeEntranceTrigger = newTrigger.GetComponent<OWTriggerVolume>();

			// Make a new box and move it so that the entrance trigger is aligned with the entrance better.
			// This prevents the player from moving with the entrance when they aren't in it
			// because the base shape is too far up.
			var box = (BoxShape)blackHoleForgeEntranceTrigger._shape;
			// The shape is rotated so we dont use Y.
			box.center = new Vector3(-1.85f, 0f, 0f);
			box.enabled = true;
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
	}
}
