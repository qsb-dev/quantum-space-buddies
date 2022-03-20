using QSB.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.SectorSync;

public class FakeSector : Sector
{
	public Sector AttachedSector => _parentSector;

	public static void CreateOn(GameObject go, Sector parent, float radius)
	{
		var name = $"{go.name}_FakeSector";
		if (go.transform.Find(name))
		{
			return;
		}

		var go2 = new GameObject(name);
		go2.SetActive(false);
		go2.transform.SetParent(go.transform, false);

		var fakeSector = go2.AddComponent<FakeSector>();
		fakeSector._name = (Name)(-1);
		fakeSector._subsectors = new List<Sector>();
		fakeSector.SetParentSector(parent);

		go2.AddComponent<OWTriggerVolume>();

		go2.AddComponent<SphereShape>().radius = radius;

		go2.AddComponent<Renderer>().FakeSector = fakeSector;

		go2.SetActive(true);

		DebugLog.DebugWrite($"fake sector {fakeSector.name} created!\n" +
			$"on go {go.name}, parent sector {parent.name}, radius {radius}");
	}

	private class Renderer : MonoBehaviour
	{
		public FakeSector FakeSector;

		private void OnRenderObject()
		{
			Popcron.Gizmos.Sphere(transform.position, 1, Color.yellow);
			var worldBounds = FakeSector.GetTriggerVolume().GetShape().CalcWorldBounds();
			Popcron.Gizmos.Sphere(worldBounds.center, worldBounds.radius, Color.yellow);
		}
	}
}
