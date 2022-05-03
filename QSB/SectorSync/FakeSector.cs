using OWML.Common;
using QSB.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.SectorSync;

public class FakeSector : Sector
{
	public static void Create(GameObject go, Sector parent, Action<FakeSector> setupSector)
	{
		var name = $"FakeSector_{go.name}";
		if (go.transform.Find(name))
		{
			DebugLog.DebugWrite($"{name} already exists", MessageType.Warning);
			return;
		}

		var go2 = new GameObject(name);
		go2.SetActive(false);
		go2.transform.SetParent(go.transform, false);

		var fakeSector = go2.AddComponent<FakeSector>();
		fakeSector._name = (Name)(-1);
		fakeSector._subsectors = new List<Sector>();
		fakeSector._idString = name;
		fakeSector.SetParentSector(parent);
		setupSector(fakeSector);

		go2.SetActive(true);
	}
}
