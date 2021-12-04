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

			foreach (var transformSync in QSBWorldSync.GetWorldObjects<OccasionalTransformSync>())
			{
				QNetworkServer.Destroy(transformSync.gameObject);
			}

			// islands
			foreach (var islandController in QSBWorldSync.GetWorldObjects<IslandController>())
			{
				var transformSync = Instantiate(QSBNetworkManager.Instance.OccasionalPrefab).GetComponent<OccasionalTransformSync>();
				transformSync.InitBodyIndexes(islandController._islandBody, Locator._giantsDeep.GetOWRigidbody());
				transformSync.gameObject.SpawnWithServerAuthority();
			}

			// cannon parts
			foreach (var partName in new[]
			{
				"OrbitalProbeCannon_Body",
				"CannonBarrel_Body",
				"CannonMuzzle_Body",
				"Debris_Body (1)",
				"Debris_Body (2)"
			})
			{
				var transformSync = Instantiate(QSBNetworkManager.Instance.OccasionalPrefab).GetComponent<OccasionalTransformSync>();
				transformSync.InitBodyIndexes(GameObject.Find(partName).GetAttachedOWRigidbody(),
					Locator._giantsDeep.GetOWRigidbody());
				transformSync.gameObject.SpawnWithServerAuthority();
			}
		}
	}
}
