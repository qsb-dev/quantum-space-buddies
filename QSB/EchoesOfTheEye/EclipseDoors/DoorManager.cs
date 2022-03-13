using Cysharp.Threading.Tasks;
using Mirror;
using QSB.EchoesOfTheEye.EclipseDoors.VariableSync;
using QSB.EchoesOfTheEye.EclipseDoors.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace QSB.EchoesOfTheEye.EclipseDoors;

internal class DoorManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public static readonly List<EclipseDoorController> Doors = new();

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		Doors.Clear();
		Doors.AddRange(QSBWorldSync.GetUnityObjects<EclipseDoorController>().SortDeterministic());
		QSBWorldSync.Init<QSBEclipseDoorController, EclipseDoorController>();

		var allDoors = QSBWorldSync.GetWorldObjects<QSBEclipseDoorController>().ToArray();

		if (QSBCore.IsHost)
		{
			foreach (var item in allDoors)
			{
				var networkObject = Instantiate(QSBNetworkManager.singleton.DoorPrefab);
				networkObject.SpawnWithServerAuthority();
			}
		}

		await UniTask.WaitUntil(() => EclipseDoorVariableSyncer.GetSpecificSyncers<EclipseDoorVariableSyncer>().Count == Doors.Count, cancellationToken: ct);

		foreach (var item in allDoors)
		{
			var index = Doors.IndexOf(item.AttachedObject);
			var syncer = EclipseDoorVariableSyncer.GetSpecificSyncers<EclipseDoorVariableSyncer>()[index];
			item.SetSyncer(syncer);
		}
	}
}
