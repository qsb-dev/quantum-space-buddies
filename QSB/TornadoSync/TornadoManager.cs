using QSB.TornadoSync.TransformSync;
using QSB.TornadoSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using QuantumUNET;

namespace QSB.TornadoSync
{
	public class TornadoManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBTornado, TornadoController>();

			if (!QSBCore.IsHost)
			{
				return;
			}

			foreach (var transformSync in QSBWorldSync.GetUnityObjects<OccasionalTransformSync>())
			{
				QNetworkServer.Destroy(transformSync.gameObject);
			}

			var gdBody = Locator._giantsDeep.GetOWRigidbody();
			// cannon
			var cannon = Locator._orbitalProbeCannon.GetRequiredComponent<OrbitalProbeLaunchController>();
			SpawnOccasional(cannon.GetAttachedOWRigidbody(), gdBody);
			foreach (var proxy in cannon._realDebrisSectorProxies)
			{
				SpawnOccasional(proxy.transform.root.GetAttachedOWRigidbody(), gdBody);
			}
			SpawnOccasional(cannon._probeBody, gdBody);

			// islands
			foreach (var island in QSBWorldSync.GetUnityObjects<IslandController>())
			{
				SpawnOccasional(island._islandBody, gdBody);
			}
		}

		private static void SpawnOccasional(OWRigidbody body, OWRigidbody refBody)
		{
			var transformSync = Instantiate(QSBNetworkManager.Instance.OccasionalPrefab).GetRequiredComponent<OccasionalTransformSync>();
			transformSync.InitBodyIndexes(body, refBody);
			transformSync.gameObject.SpawnWithServerAuthority();
		}
	}
}
