using System.Collections.Generic;
using UnityEngine;

namespace QSB.ShipSync;

public static class ShipThrusterManager
{
	public static List<ThrusterFlameController> ShipFlameControllers = new();
	public static ThrusterWashController ShipWashController = new();

	public static void CreateShipVFX()
	{
		var shipBody = Locator.GetShipBody();
		var Module_Cabin = shipBody.transform.Find("Module_Cabin");
		var Effects_Cabin = Module_Cabin.Find("Effects_Cabin");
		var ThrusterWash = Effects_Cabin.Find("ThrusterWash");

		ShipWashController = ThrusterWash.GetComponent<ThrusterWashController>();

		var Module_Supplies = shipBody.transform.Find("Module_Supplies");
		var Effects_Supplies = Module_Supplies.Find("Effects_Supplies");
		var SuppliesThrusters = Effects_Supplies.Find("Thrusters");
		ShipFlameControllers.Clear();
		foreach (Transform thruster in SuppliesThrusters)
		{
			if (thruster.name == "Particles")
			{
				continue;
			}

			var flame = thruster.GetChild(0);
			ShipFlameControllers.Add(flame.GetComponent<ThrusterFlameController>());
		}

		var Module_Engine = shipBody.transform.Find("Module_Engine");
		var Effects_Engine = Module_Engine.Find("Effects_Engine");
		var EngineThrusters = Effects_Engine.Find("Thrusters");
		foreach (Transform thruster in EngineThrusters)
		{
			if (thruster.name == "Particles")
			{
				continue;
			}

			var flame = thruster.GetChild(0);
			ShipFlameControllers.Add(flame.GetComponent<ThrusterFlameController>());
		}
	}
}
