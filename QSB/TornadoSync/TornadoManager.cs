using System.Linq;
using QSB.TornadoSync.TransformSync;
using QSB.TornadoSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;
using UnityEngine;

namespace QSB.TornadoSync
{
	public class TornadoManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBTornado, TornadoController>();

			if (!QSBCore.IsHost)
			{
				return;
			}

			foreach (var transformSync in QSBWorldSync.GetWorldObjects<OccasionalTransformSync>())
			{
				QNetworkServer.Destroy(transformSync.gameObject);
			}

			// cannon parts
			foreach (var cannonPartNames in new[]
			{
				"OrbitalProbeCannon_Body",
				"CannonBarrel_Body",
				"CannonMuzzle_Body"
			})
			{
				var cannonPart = GameObject.Find(cannonPartNames).GetAttachedOWRigidbody();
				var transformSync = Instantiate(QSBNetworkManager.Instance.OccasionalPrefab).GetComponent<OccasionalTransformSync>();
				transformSync.InitBodyIndexes(cannonPart, Locator._giantsDeep.GetOWRigidbody());
				transformSync.gameObject.SpawnWithServerAuthority();
			}

			// islands
			foreach (var islandNames in new[]
			{
				"GabbroIsland_Body",
				"StatueIsland_Body",
				"ConstructionYardIsland_Body",
				"BrambleIsland_Body"
			})
			{
				var island = GameObject.Find(islandNames).GetAttachedOWRigidbody();
				var transformSync = Instantiate(QSBNetworkManager.Instance.OccasionalPrefab).GetComponent<OccasionalTransformSync>();
				transformSync.InitBodyIndexes(island, Locator._giantsDeep.GetOWRigidbody());
				transformSync.gameObject.SpawnWithServerAuthority();
			}
		}
	}
}
