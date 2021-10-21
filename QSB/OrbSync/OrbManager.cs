using OWML.Common;
using QSB.OrbSync.TransformSync;
using QSB.OrbSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QSB.OrbSync
{
	public class OrbManager : WorldObjectManager
	{
		private List<GameObject> _orbs = new List<GameObject>();

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
				_orbs.ForEach(x => QNetworkServer.Destroy(x));
				_orbs.Clear();
				NomaiOrbTransformSync.OrbTransformSyncs.Clear();
				foreach (var orb in QSBWorldSync.OldOrbList)
				{
					var newOrb = Instantiate(QSBNetworkManager.Instance.OrbPrefab);
					newOrb.SpawnWithServerAuthority();
				}
			}

			DebugLog.DebugWrite($"Finished orb build with {QSBWorldSync.OldOrbList.Count} orbs.", MessageType.Success);
		}
	}
}