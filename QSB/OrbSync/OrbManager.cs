using OWML.Common;
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
			QSBWorldSync.RemoveWorldObjects<QSBOrbSlot>();
			QSBWorldSync.Init<QSBOrbSlot, NomaiInterfaceSlot>();
		}

		public void BuildOrbs()
		{
			QSBWorldSync.OldOrbList.Clear();
			QSBWorldSync.OldOrbList = Resources.FindObjectsOfTypeAll<NomaiInterfaceOrb>().ToList();
			if (QNetworkServer.active)
			{
				QSBWorldSync.OrbSyncList.ForEach(x => QNetworkServer.Destroy(x.gameObject));
				QSBWorldSync.OrbSyncList.Clear();
				QSBWorldSync.OldOrbList.ForEach(x => QNetworkServer.Spawn(Instantiate(QSBNetworkManager.Instance.OrbPrefab)));
			}
			DebugLog.DebugWrite($"Finished orb build with {QSBWorldSync.OldOrbList.Count} orbs.", MessageType.Success);
		}

		public void QueueBuildSlots() => QSBCore.Helper.Events.Unity.RunWhen(() => QSBCore.HasWokenUp, BuildOrbSlots);
		public void QueueBuildOrbs() => QSBCore.Helper.Events.Unity.RunWhen(() => QNetworkServer.active, BuildOrbs);
	}
}