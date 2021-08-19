using OWML.Common;
using QSB.OrbSync.TransformSync;
using QSB.OrbSync.WorldObjects;
using QSB.Player;
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
			if (QSBCore.IsHost)
			{
				NomaiOrbTransformSync.OrbTransformSyncs.ForEach(x => QNetworkServer.Destroy(x.gameObject));
				NomaiOrbTransformSync.OrbTransformSyncs.Clear();
				foreach (var orb in QSBWorldSync.OldOrbList)
				{
					Instantiate(QSBNetworkManager.Instance.OrbPrefab).SpawnWithServerAuthority();
				}
			}

			DebugLog.DebugWrite($"Finished orb build with {QSBWorldSync.OldOrbList.Count} orbs.", MessageType.Success);
		}
	}
}