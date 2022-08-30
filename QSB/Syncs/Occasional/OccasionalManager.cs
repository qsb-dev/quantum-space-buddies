using Cysharp.Threading.Tasks;
using Mirror;
using OWML.Common;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Threading;

namespace QSB.Syncs.Occasional;

internal class OccasionalManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public static readonly List<(OWRigidbody Body, OWRigidbody RefBody)> Bodies = new();

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		var gdBody = Locator._giantsDeep.GetOWRigidbody();
		var cannon = Locator._orbitalProbeCannon.GetRequiredComponent<OrbitalProbeLaunchController>();
		if (!cannon) DebugLog.DebugWrite(" CANNON IS NULL!", MessageType.Error);
		if (!cannon.GetAttachedOWRigidbody()) DebugLog.DebugWrite(" CANNON BODY IS NULL!", MessageType.Error);
		SpawnOccasional(cannon.GetAttachedOWRigidbody(), gdBody);

		foreach (var proxy in cannon._realDebrisSectorProxies)
		{
			// NH can remove these
			if (!proxy)
			{
				continue;
			}
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
		if (!body) DebugLog.DebugWrite($"BODY IS NULL {Environment.StackTrace}", MessageType.Error);
		if (!refBody) DebugLog.DebugWrite($"BODY IS NULL {Environment.StackTrace}", MessageType.Error);

		if (QSBCore.IsHost)
		{
			Instantiate(QSBNetworkManager.singleton.OccasionalPrefab).SpawnWithServerAuthority();
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

		Bodies.Clear();
	}
}
