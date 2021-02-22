using OWML.Common;
using OWML.Utils;
using QSB.OrbSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using System.Linq;
using UnityEngine;

namespace QSB.OrbSync
{
	public class OrbManager : MonoBehaviour
	{
		public static OrbManager Instance { get; private set; }

		private void Awake() => Instance = this;

		private void BuildOrbSlots()
		{
			WorldObjectManager.RemoveWorldObjects<QSBOrbSlot>();
			WorldObjectManager.Init<QSBOrbSlot, NomaiInterfaceSlot>();
		}

		public void BuildOrbs()
		{
			WorldObjectManager.OldOrbList.Clear();
			WorldObjectManager.OldOrbList = Resources.FindObjectsOfTypeAll<NomaiInterfaceOrb>().ToList();
			if (QNetworkServer.active)
			{
				WorldObjectManager.OrbSyncList.ForEach(x => QNetworkServer.Destroy(x.gameObject));
				WorldObjectManager.OrbSyncList.Clear();
				WorldObjectManager.OldOrbList.ForEach(x => QNetworkServer.Spawn(Instantiate(QSBNetworkManager.Instance.OrbPrefab)));
			}
			DebugLog.DebugWrite($"Finished orb build with {WorldObjectManager.OldOrbList.Count} orbs.", MessageType.Success);
		}

		public void OnRenderObject()
		{
			if (!QSBCore.HasWokenUp || !QSBCore.DebugMode || !QSBCore.ShowLinesInDebug)
			{
				return;
			}

			foreach (var orb in WorldObjectManager.OldOrbList)
			{
				Popcron.Gizmos.Cube(orb.transform.position, orb.transform.rotation, Vector3.one / 3);

				var rails = orb.GetValue<OWRail[]>("_safetyRails");
				if (rails.Length > 0)
				{
					foreach (var rail in rails)
					{
						var points = rail.GetValue<Vector3[]>("_railPoints");
						for (var i = 0; i < points.Length; i++)
						{
							if (i > 0)
							{
								Popcron.Gizmos.Line(rail.transform.TransformPoint(points[i - 1]), rail.transform.TransformPoint(points[i]), Color.white);
							}
						}
					}
				}
			}
		}

		public void QueueBuildSlots() => QSBCore.Helper.Events.Unity.RunWhen(() => QSBCore.HasWokenUp, BuildOrbSlots);
		public void QueueBuildOrbs() => QSBCore.Helper.Events.Unity.RunWhen(() => QNetworkServer.active, BuildOrbs);
	}
}