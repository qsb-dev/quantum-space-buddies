using OWML.Common;
using OWML.Utils;
using QSB.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace QSB.ShipSync
{
	class ShipManager : MonoBehaviour
	{
		public static ShipManager Instance;

		public InteractZone HatchInteractZone;
		public HatchController HatchController;
		public ShipTractorBeamSwitch ShipTractorBeam;

		private uint _currentFlyer = uint.MaxValue;
		public uint CurrentFlyer
		{
			get => _currentFlyer;
			set
			{
				if (_currentFlyer != uint.MaxValue && value != uint.MaxValue)
				{
					DebugLog.ToConsole($"Warning - Trying to set current flyer while someone is still flying? Current:{_currentFlyer}, New:{value}", MessageType.Warning);
				}
				_currentFlyer = value;
			}
		}

		private void Awake()
		{
			QSBSceneManager.OnUniverseSceneLoaded += OnSceneLoaded;
			Instance = this;

			var shipTransform = Locator.GetShipTransform();
			HatchController = shipTransform.GetComponentInChildren<HatchController>();
			HatchInteractZone = HatchController.GetComponent<InteractZone>();
			ShipTractorBeam = Resources.FindObjectsOfTypeAll<ShipTractorBeamSwitch>().First();
		}

		private void OnSceneLoaded(OWScene scene)
		{
			if (scene == OWScene.EyeOfTheUniverse)
			{
				return;
			}
			HatchInteractZone.SetValue("_viewingWindow", 90f);

			var sphereShape = HatchController.GetComponent<SphereShape>();
			sphereShape.radius = 2.5f;
			sphereShape.center = new Vector3(0, 0, 1);
		}
	}
}
