using Mirror;
using QSB.TornadoSync.TransformSync;
using QSB.TornadoSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;

namespace QSB.TornadoSync
{
	public class TornadoManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		public override void BuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBTornado, TornadoController>();

			var gdBody = Locator._giantsDeep.GetOWRigidbody();
			// cannon
			var cannon = Locator._orbitalProbeCannon.GetRequiredComponent<OrbitalProbeLaunchController>();
			SpawnOccasional(cannon.GetAttachedOWRigidbody(), gdBody);
			foreach (var proxy in cannon._realDebrisSectorProxies)
			{
				SpawnOccasional(proxy.transform.root.GetAttachedOWRigidbody(), gdBody);
			}

			if (cannon._probeBody)
			{
				SpawnOccasional(cannon._probeBody, gdBody);
			}

			// islands
			foreach (var island in QSBWorldSync.GetUnityObjects<IslandController>().SortDeterministic())
			{
				SpawnOccasional(island._islandBody, gdBody);
			}
		}

		public override void UnbuildWorldObjects()
		{
			if (QSBCore.IsHost)
			{
				foreach (var transformSync in QSBWorldSync.GetUnityObjects<OccasionalTransformSync>())
				{
					NetworkServer.Destroy(transformSync.gameObject);
				}
			}

			OccasionalTransformSync.Bodies.Clear();
		}

		private static void SpawnOccasional(OWRigidbody body, OWRigidbody refBody)
		{
			OccasionalTransformSync.Bodies.Add((body, refBody));
			if (QSBCore.IsHost)
			{
				var transformSync = Instantiate(QSBNetworkManager.singleton.OccasionalPrefab).GetRequiredComponent<OccasionalTransformSync>();
				transformSync.gameObject.SpawnWithServerAuthority();
			}
		}
	}
}
