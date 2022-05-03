﻿using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;
using QSB.RespawnSync;
using QSB.ShipSync;
using QSB.Utility.Messages;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QSB.Utility;

public class DebugActions : MonoBehaviour, IAddComponentOnStart
{
	private static void GoToVessel()
	{
		var spawnPoint = GameObject.Find("Spawn_Vessel").GetComponent<SpawnPoint>();

		var playerBody = Locator.GetPlayerBody();
		playerBody.WarpToPositionRotation(spawnPoint.transform.position, spawnPoint.transform.rotation);
		playerBody.SetVelocity(spawnPoint.GetPointVelocity());
		var bridgeVolume = FindObjectOfType<VesselWarpController>()._bridgeVolume;
		bridgeVolume.AddObjectToVolume(Locator.GetPlayerDetector());
		bridgeVolume.AddObjectToVolume(Locator.GetPlayerCameraDetector());
	}

	private static void InsertWarpCore()
	{
		var warpCore = GameObject.Find("Prefab_NOM_WarpCoreVessel").GetComponent<WarpCoreItem>();
		var socket = GameObject.Find("Interactibles_VesselBridge").GetComponentInChildren<WarpCoreSocket>();
		socket.PlaceIntoSocket(warpCore);
	}

	private static void DamageShipElectricalSystem() =>
		ShipManager.Instance.ShipElectricalComponent.SetDamaged(true);

	private void Awake() => enabled = QSBCore.DebugSettings.DebugMode;

	private int _otherPlayerToTeleportTo;

	public void Update()
	{
		if (!Keyboard.current[Key.Q].isPressed)
		{
			return;
		}

		/*
		 * 1 - Warp to first non local player
		 * 2 - Enter dream world
		 * 3 - Destroy probe
		 * 4 - Damage ship electricals
		 * 5 - Trigger supernova
		 * 6 - Set MET_SOLANUM
		 * 7 - Warp to vessel
		 * 8 - Place warp core into vessel
		 * 9 - Load eye scene
		 * 0 - Respawn some player
		 */

		if (Keyboard.current[Key.Numpad1].wasPressedThisFrame)
		{
			var otherPlayers = QSBPlayerManager.PlayerList.Where(x => !x.IsLocalPlayer).ToList();
			_otherPlayerToTeleportTo = (_otherPlayerToTeleportTo + 1) % otherPlayers.Count;
			var otherPlayer = otherPlayers[_otherPlayerToTeleportTo];
			new DebugRequestTeleportInfoMessage(otherPlayer.PlayerId).Send();
		}

		if (Keyboard.current[Key.Numpad2].wasPressedThisFrame)
		{
			if (!QSBPlayerManager.LocalPlayer.InDreamWorld)
			{
				var relativeLocation = new RelativeLocationData(Vector3.up * 2 + Vector3.forward * 2, Quaternion.identity, Vector3.zero);

				var location = Keyboard.current[Key.LeftShift].isPressed ? DreamArrivalPoint.Location.Zone4 : DreamArrivalPoint.Location.Zone3;
				var arrivalPoint = Locator.GetDreamArrivalPoint(location);
				var dreamCampfire = Locator.GetDreamCampfire(location);
				if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() != ItemType.DreamLantern)
				{
					var dreamLanternItem = QSBWorldSync.GetWorldObjects<QSBDreamLanternItem>().First(x =>
						x.AttachedObject._lanternType == DreamLanternType.Functioning &&
						QSBPlayerManager.PlayerList.All(y => y.HeldItem != x)
					).AttachedObject;
					Locator.GetToolModeSwapper().GetItemCarryTool().PickUpItemInstantly(dreamLanternItem);
				}

				Locator.GetDreamWorldController().EnterDreamWorld(dreamCampfire, arrivalPoint, relativeLocation);
			}
			else
			{
				if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() != ItemType.DreamLantern)
				{
					var dreamLanternItem = QSBPlayerManager.LocalPlayer.AssignedSimulationLantern.AttachedObject;
					Locator.GetToolModeSwapper().GetItemCarryTool().PickUpItemInstantly(dreamLanternItem);
				}
			}
		}

		if (Keyboard.current[Key.Numpad3].wasPressedThisFrame)
		{
			var sarcoController = QSBWorldSync.GetUnityObject<SarcophagusController>();

			sarcoController.firstSealProjector.SetLit(false);
			sarcoController.secondSealProjector.SetLit(false);
			sarcoController.thirdSealProjector.SetLit(false);

			sarcoController._attemptOpenAfterDelay = true;
			sarcoController._openAttemptTime = Time.time + 0.5f;
			sarcoController.enabled = true;
		}

		if (Keyboard.current[Key.Numpad4].wasPressedThisFrame)
		{
			DamageShipElectricalSystem();
		}

		if (Keyboard.current[Key.Numpad5].wasPressedThisFrame)
		{
			new DebugTriggerSupernovaMessage().Send();
		}

		if (Keyboard.current[Key.Numpad6].wasPressedThisFrame)
		{
			PlayerData.SetPersistentCondition("MET_SOLANUM", true);
			PlayerData.SetPersistentCondition("MET_PRISONER", true);
			DialogueConditionManager.SharedInstance.SetConditionState("MET_SOLANUM", true);
			DialogueConditionManager.SharedInstance.SetConditionState("MET_PRISONER", true);
		}

		if (Keyboard.current[Key.Numpad7].wasPressedThisFrame)
		{
			GoToVessel();
		}

		if (Keyboard.current[Key.Numpad8].wasPressedThisFrame)
		{
			InsertWarpCore();
		}

		if (Keyboard.current[Key.Numpad9].wasPressedThisFrame)
		{
			new DebugChangeSceneMessage(Keyboard.current[Key.LeftShift].isPressed).Send();
		}

		if (Keyboard.current[Key.Numpad0].wasPressedThisFrame)
		{
			RespawnManager.Instance.RespawnSomePlayer();
		}
	}
}
