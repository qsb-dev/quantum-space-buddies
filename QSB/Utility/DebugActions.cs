using OWML.Common;
using QSB.EchoesOfTheEye.DreamLantern;
using QSB.EchoesOfTheEye.DreamLantern.WorldObjects;
using QSB.ItemSync.WorldObjects.Items;
using QSB.Messaging;
using QSB.Player;
using QSB.QuantumSync.WorldObjects;
using QSB.RespawnSync;
using QSB.ShipSync;
using QSB.Utility.Messages;
using QSB.WorldSync;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QSB.Utility;

public class DebugActions : MonoBehaviour, IAddComponentOnStart
{
	public static Type WorldObjectSelection = typeof(QSBSocketedQuantumObject);

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
	private int _backTimer;
	private int _forwardTimer;

	private const int UpdatesUntilScroll = 30;
	private const int UpdatesBetweenScroll = 5;

	private static void GoForwardOneObject()
	{
		var allWorldObjects = typeof(IWorldObject).GetDerivedTypes().ToArray();
		if (WorldObjectSelection == null)
		{
			WorldObjectSelection = allWorldObjects.First();
			return;
		}

		var index = Array.IndexOf(allWorldObjects, WorldObjectSelection) + 1;

		if (index == allWorldObjects.Length)
		{
			index = 0;
		}

		WorldObjectSelection = allWorldObjects[index];
	}

	private static void GoBackOneObject()
	{
		var allWorldObjects = typeof(IWorldObject).GetDerivedTypes().ToArray();
		if (WorldObjectSelection == null)
		{
			WorldObjectSelection = allWorldObjects.Last();
			return;
		}

		var index = Array.IndexOf(allWorldObjects, WorldObjectSelection) - 1;

		if (index < 0)
		{
			index = allWorldObjects.Length - 1;
		}

		WorldObjectSelection = allWorldObjects[index];
	}

	public void Update()
	{
		if (!Keyboard.current[Key.Q].isPressed)
		{
			return;
		}

		if (Keyboard.current[Key.Comma].isPressed && Keyboard.current[Key.Period].isPressed)
		{
			WorldObjectSelection = null;
		}
		else if (Keyboard.current[Key.Comma].wasPressedThisFrame)
		{
			GoBackOneObject();
		}
		else if (Keyboard.current[Key.Period].wasPressedThisFrame)
		{
			GoForwardOneObject();
		}
		else
		{
			if (Keyboard.current[Key.Comma].isPressed)
			{
				_backTimer++;

				if (_backTimer >= UpdatesUntilScroll)
				{
					if (_backTimer == UpdatesUntilScroll + UpdatesBetweenScroll)
					{
						_backTimer = UpdatesUntilScroll;
						GoBackOneObject();
					}
				}
			}
			else
			{
				_backTimer = 0;
			}

			if (Keyboard.current[Key.Period].isPressed)
			{
				_forwardTimer++;

				if (_forwardTimer >= UpdatesUntilScroll)
				{
					if (_forwardTimer == UpdatesUntilScroll + UpdatesBetweenScroll)
					{
						_forwardTimer = UpdatesUntilScroll;
						GoForwardOneObject();
					}
				}
			}
			else
			{
				_forwardTimer = 0;
			}
		}

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
				// modified from DayDream debug thing
				var relativeLocation = new RelativeLocationData(Vector3.up * 2 + Vector3.forward * 2, Quaternion.identity, Vector3.zero);

				var location = Keyboard.current[Key.LeftShift].isPressed ? DreamArrivalPoint.Location.Zone4 : DreamArrivalPoint.Location.Zone3;
				var arrivalPoint = Locator.GetDreamArrivalPoint(location);
				var dreamCampfire = Locator.GetDreamCampfire(location);
				if (Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItemType() != ItemType.DreamLantern)
				{
					var dreamLanternItem = QSBWorldSync.GetWorldObjects<QSBDreamLanternItem>().First(x =>
						x.AttachedObject._lanternType == DreamLanternType.Functioning &&
						QSBPlayerManager.PlayerList.All(y => y.HeldItem != x) &&
						!x.AttachedObject.GetLanternController().IsLit()
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

			sarcoController.OnPressInteract();
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
			InsertWarpCore();
		}

		if (Keyboard.current[Key.Numpad8].wasPressedThisFrame)
		{
			var player = new PlayerInfo(QSBPlayerManager.LocalPlayer.TransformSync);
			QSBPlayerManager.PlayerList.SafeAdd(player);
			QSBPlayerManager.OnAddPlayer?.SafeInvoke(player);
			DebugLog.DebugWrite($"CREATING FAKE PLAYER : {player}", MessageType.Info);

			JoinLeaveSingularity.Create(player, true);
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
