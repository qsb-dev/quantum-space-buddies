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
			var mainCannonPart = GameObject.Find("OrbitalProbeCannon_Body").GetAttachedOWRigidbody();
			{
				var transformSync = Instantiate(QSBNetworkManager.Instance.OccasionalPrefab).GetComponent<OccasionalTransformSync>();
				transformSync.InitBodyIndexes(mainCannonPart, Locator._giantsDeep.GetOWRigidbody());
				transformSync.gameObject.SpawnWithServerAuthority();
			}
			foreach (var cannonPartNames in new[]
			{
				"CannonBarrel_Body",
				"CannonMuzzle_Body",
				"Debris_Body (1)",
				"Debris_Body (2)"
			})
			{
				var cannonPart = GameObject.Find(cannonPartNames).GetAttachedOWRigidbody();
				var transformSync = Instantiate(QSBNetworkManager.Instance.OccasionalPrefab).GetComponent<OccasionalTransformSync>();
				transformSync.InitBodyIndexes(cannonPart, mainCannonPart);
				transformSync.gameObject.SpawnWithServerAuthority();
			}

			// islands
			var islandControllers = QSBWorldSync.GetWorldObjects<IslandController>().ToArray();
			DebugLog.DebugWrite($"there are {islandControllers.Length} islandControllers");
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
