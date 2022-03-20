using OWML.Common;
using QSB.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.SectorSync;

public class FakeSector : Sector
{
	public float Radius;

	public static void CreateOn(GameObject go, float radius, Sector parent)
	{
		var name = $"{go.name}_FakeSector";
		if (go.transform.Find(name))
		{
			DebugLog.DebugWrite($"fake sector {name} already exists", MessageType.Warning);
			return;
		}

		var go2 = new GameObject(name);
		go2.SetActive(false);
		go2.transform.SetParent(go.transform, false);

		var fakeSector = go2.AddComponent<FakeSector>();
		fakeSector._subsectors = new List<Sector>();
		fakeSector.Radius = radius;
		fakeSector.SetParentSector(parent);

		go2.AddComponent<OWTriggerVolume>();
		go2.AddComponent<SphereShape>().radius = fakeSector.Radius;
		// go2.AddComponent<DebugRenderer>().FakeSector = fakeSector;

		go2.SetActive(true);
	}

	private class DebugRenderer : MonoBehaviour
	{
		[NonSerialized]
		public FakeSector FakeSector;

		private void OnRenderObject()
		{
			if (!QSBCore.DebugSettings.DebugMode)
			{
				return;
			}

			Popcron.Gizmos.Sphere(FakeSector.transform.position, FakeSector.Radius, Color.yellow);
		}

		private void OnGUI()
		{
			if (!QSBCore.DebugSettings.DebugMode ||
				Event.current.type != EventType.Repaint)
			{
				return;
			}

			DebugGUI.DrawLabel(FakeSector.transform,
				$"{FakeSector.name}\n" +
				$"{FakeSector._parentSector.name} | {FakeSector.Radius}");
		}
	}
}
