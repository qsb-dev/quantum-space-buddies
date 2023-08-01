using Cysharp.Threading.Tasks;
using Mirror;
using QSB.Utility;
using QSB.WorldSync;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QSB.Syncs.Occasional;

public class OccasionalManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public static readonly List<(OWRigidbody Body, OWRigidbody RefBody)> Bodies = new();

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		var gdBody = Locator._giantsDeep.GetOWRigidbody();
		var cannon = Locator._orbitalProbeCannon.GetRequiredComponent<OrbitalProbeLaunchController>();
		SpawnOccasional(cannon.GetAttachedOWRigidbody(), gdBody);

		foreach (var proxy in cannon._realDebrisSectorProxies)
		{
			SpawnOccasional(proxy.transform.root.GetAttachedOWRigidbody(), gdBody);
		}

		if (cannon._probeBody)
		{
			// probe is null on statue scene reload
			SpawnOccasional(cannon._probeBody, gdBody);
		}

		foreach (var island in QSBWorldSync.GetUnityObjects<IslandController>().SortDeterministic())
		{
			SpawnOccasional(island._islandBody, gdBody);
		}
	}

	public static void SpawnOccasional(OWRigidbody body, OWRigidbody refBody)
	{
		Bodies.Add((body, refBody));

		if (QSBCore.IsHost)
		{
			Instantiate(QSBNetworkManager.singleton.OccasionalPrefab).SpawnWithServerOwnership();
		}
	}

	public override void UnbuildWorldObjects()
	{
		if (QSBCore.IsHost)
		{
			foreach (var transformSync in OccasionalTransformSync.Instances.ToList())
			{
				NetworkServer.Destroy(transformSync.gameObject);
			}
		}

		Bodies.Clear();
	}
}
