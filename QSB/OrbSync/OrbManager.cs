using OWML.Common;
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

		private void Awake()
		{
			Instance = this;
		}

		private void BuildOrbSlots()
		{
			DebugLog.DebugWrite("Building QSBOrbSlots...", MessageType.Info);
			QSBWorldSync.RemoveWorldObjects<QSBOrbSlot>();
			var orbSlots = Resources.FindObjectsOfTypeAll<NomaiInterfaceSlot>();
			for (var id = 0; id < orbSlots.Length; id++)
			{
				var qsbOrbSlot = QSBWorldSync.GetWorldObject<QSBOrbSlot>(id) ?? new QSBOrbSlot();
				qsbOrbSlot.Init(orbSlots[id], id);
			}

			DebugLog.DebugWrite($"Finished orb slot build with {orbSlots.Length} slots.", MessageType.Success);
		}

		public void BuildOrbs()
		{
			DebugLog.DebugWrite("Building orb syncs...", MessageType.Info);
			QSBWorldSync.OldOrbList.Clear();
			QSBWorldSync.OldOrbList = Resources.FindObjectsOfTypeAll<NomaiInterfaceOrb>().ToList();
			if (QSBNetworkServer.active)
			{
				DebugLog.DebugWrite("- Is server, instantiating orb prefabs.");
				QSBWorldSync.OrbSyncList.ForEach(x => QSBNetworkServer.Destroy(x.gameObject));
				QSBWorldSync.OrbSyncList.Clear();
				QSBWorldSync.OldOrbList.ForEach(x => QSBNetworkServer.Spawn(Instantiate(QSBNetworkManager.Instance.OrbPrefab)));
			}
			DebugLog.DebugWrite($"Finished orb build with {QSBWorldSync.OldOrbList.Count} orbs.", MessageType.Success);
		}

		public void QueueBuildSlots()
		{
			DebugLog.DebugWrite("Queueing build of QSBOrbSlots...", MessageType.Info);
			QSB.Helper.Events.Unity.RunWhen(() => QSB.HasWokenUp, BuildOrbSlots);
		}

		public void QueueBuildOrbs()
		{
			DebugLog.DebugWrite("Queueing build of NetworkOrbs...", MessageType.Info);
			QSB.Helper.Events.Unity.RunWhen(() => QSBNetworkServer.active, BuildOrbs);
		}
	}
}