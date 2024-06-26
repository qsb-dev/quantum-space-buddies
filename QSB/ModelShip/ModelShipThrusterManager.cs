﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.ModelShip;

public static class ModelShipThrusterManager
{
	public static readonly List<ThrusterFlameController> ThrusterFlameControllers = new();
	public static ThrusterWashController ThrusterWashController { get; private set; }

	public static void CreateModelShipVFX(GameObject modelShip)
	{
		ThrusterFlameControllers.Clear();
		foreach (var item in modelShip.GetComponentsInChildren<ThrusterFlameController>())
		{
			ThrusterFlameControllers.Add(item);
		}

		ThrusterWashController = modelShip.GetComponentInChildren<ThrusterWashController>();
	}
}
