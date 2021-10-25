using OWML.Utils;
using QSB.Events;
using QSB.ShipSync;
using QSB.Utility.Events;
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
		}

		private void InsertWarpCore()
		{
			var warpCore = GameObject.Find("Prefab_NOM_WarpCoreVessel").GetComponent<WarpCoreItem>();
			var socket = GameObject.Find("Interactibles_VesselBridge").GetComponentInChildren<WarpCoreSocket>();
			socket.PlaceIntoSocket(warpCore);
			var bridgeVolume = FindObjectOfType<VesselWarpController>().GetValue<OWTriggerVolume>("_bridgeVolume");
			bridgeVolume.AddObjectToVolume(Locator.GetPlayerDetector());
			bridgeVolume.AddObjectToVolume(Locator.GetPlayerCameraDetector());
		}

		private void DamageShipElectricalSystem() => ShipManager.Instance.ShipElectricalComponent.SetDamaged(true);

		public void Update()
		{
			if (!QSBCore.DebugMode)
			{
				return;
			}

			if (Keyboard.current[Key.Numpad4].wasPressedThisFrame)
			{
				DamageShipElectricalSystem();
			}

			if (Keyboard.current[Key.Numpad5].wasPressedThisFrame)
			{
				QSBEventManager.FireEvent(EventNames.QSBDebugEvent, DebugEventEnum.TriggerSupernova);
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
				LoadManager.LoadSceneAsync(OWScene.EyeOfTheUniverse, true, LoadManager.FadeType.ToWhite);
			}
		}
	}
}