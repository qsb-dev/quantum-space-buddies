using OWML.Common;
using QSB.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QSB.SectorSync;

public class FakeSector : Sector
{
	public static void Create<T>(GameObject go, Sector parent, Action<T> initShape)
		where T : Shape
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
		fakeSector.SetParentSector(parent);

		go2.AddComponent<OWTriggerVolume>();
		initShape(go2.AddComponent<T>());
		go2.AddComponent<DebugRenderer>().FS = fakeSector;

		go2.SetActive(true);
	}

	private class DebugRenderer : MonoBehaviour
	{
		[NonSerialized]
		public FakeSector FS;

		private void OnRenderObject()
		{
			if (!QSBCore.DebugSettings.DebugMode)
			{
				return;
			}

			var shape = FS._owTriggerVolume._shape;
			var center = shape.GetWorldSpaceCenter();
			switch (shape)
			{
				case SphereShape sphereShape:
					Popcron.Gizmos.Sphere(center, sphereShape.radius, Color.yellow);
					break;
				case BoxShape boxShape:
					Popcron.Gizmos.Cube(center, boxShape.transform.rotation, boxShape.size, Color.yellow);
					break;
			}
		}

		private void OnGUI()
		{
			if (!QSBCore.DebugSettings.DebugMode ||
				Event.current.type != EventType.Repaint)
			{
				return;
			}

			DebugGUI.DrawLabel(FS.transform, FS.name);
		}
	}
}
