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
	public class OrbManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBOrbSlot, NomaiInterfaceSlot>();
			DebugLog.DebugWrite($"Finished slot build with {QSBWorldSync.GetWorldObjects<QSBOrbSlot>().Count()} slots.", MessageType.Success);
			BuildOrbs();
		}

		private void BuildOrbs()
		{
			QSBWorldSync.OldOrbList.Clear();
			QSBWorldSync.OldOrbList = Resources.FindObjectsOfTypeAll<NomaiInterfaceOrb>().ToList();
			if (QSBCore.IsServer)
			{
				QSBWorldSync.OrbSyncList.ForEach(x => QNetworkServer.Destroy(x.gameObject));
				QSBWorldSync.OrbSyncList.Clear();
				QSBWorldSync.OldOrbList.ForEach(x => QNetworkServer.Spawn(Instantiate(QSBNetworkManager.Instance.OrbPrefab)));
			}
			DebugLog.DebugWrite($"Finished orb build with {QSBWorldSync.OldOrbList.Count} orbs.", MessageType.Success);
		}
	}
}