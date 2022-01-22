using QSB.Messaging;
using QSB.Player;
using QSB.ShipSync;
using QSB.Utility.Messages;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QSB.Utility
{
	public class DebugActions : MonoBehaviour
	{
		private void GoToVessel()
		{
			var spawnPoint = GameObject.Find("Spawn_Vessel").GetComponent<SpawnPoint>();

			var playerBody = Locator.GetPlayerBody();
			playerBody.WarpToPositionRotation(spawnPoint.transform.position, spawnPoint.transform.rotation);
			playerBody.SetVelocity(spawnPoint.GetPointVelocity());
			var bridgeVolume = FindObjectOfType<VesselWarpController>()._bridgeVolume;
			bridgeVolume.AddObjectToVolume(Locator.GetPlayerDetector());
			bridgeVolume.AddObjectToVolume(Locator.GetPlayerCameraDetector());
		}

		private void InsertWarpCore()
		{
			var warpCore = GameObject.Find("Prefab_NOM_WarpCoreVessel").GetComponent<WarpCoreItem>();
			var socket = GameObject.Find("Interactibles_VesselBridge").GetComponentInChildren<WarpCoreSocket>();
			socket.PlaceIntoSocket(warpCore);
		}

		private void DamageShipElectricalSystem() => ShipManager.Instance.ShipElectricalComponent.SetDamaged(true);

		private void Awake() => enabled = QSBCore.DebugMode;

		public void Update()
		{
			if (!Keyboard.current[Key.Q].isPressed)
			{
				return;
			}

			/*
			 * 1 - Warp to first non local player
			 * 2 - Set time flowing
			 * 3 - Destroy probe
			 * 4 - Damage ship electricals
			 * 5 - Trigger supernova
			 * 6 - Set MET_SOLANUM
			 * 7 - Warp to vessel
			 * 8 - Place warp core into vessel
			 * 9 - Load eye scene
			 * 0 -
			 */

			if (Keyboard.current[Key.Numpad1].wasPressedThisFrame)
			{
				var otherPlayer = QSBPlayerManager.PlayerList.FirstOrDefault(x => x != QSBPlayerManager.LocalPlayer);
				if (otherPlayer != null)
				{
					new DebugRequestTeleportInfoMessage(otherPlayer.PlayerId).Send();
				}
			}

			if (Keyboard.current[Key.Numpad2].wasPressedThisFrame)
			{
				TimeLoop._isTimeFlowing = true;
			}

			if (Keyboard.current[Key.Numpad3].wasPressedThisFrame)
			{
				Destroy(Locator.GetProbe().gameObject);
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
				if (Keyboard.current[Key.LeftShift].isPressed)
				{
					PlayerData._currentGameSave.warpedToTheEye = false;
					PlayerData.SaveCurrentGame();
					LoadManager.LoadSceneAsync(OWScene.SolarSystem, true, LoadManager.FadeType.ToBlack);
				}
				else
				{
					PlayerData.SaveWarpedToTheEye(60);
					LoadManager.LoadSceneAsync(OWScene.EyeOfTheUniverse, true, LoadManager.FadeType.ToWhite);
				}
			}
		}
	}
}
