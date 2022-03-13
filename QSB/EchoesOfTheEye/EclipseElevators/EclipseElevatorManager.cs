using Cysharp.Threading.Tasks;
using Mirror;
using QSB.EchoesOfTheEye.EclipseDoors.VariableSync;
using QSB.EchoesOfTheEye.EclipseDoors.WorldObjects;
using QSB.EchoesOfTheEye.EclipseElevators.VariableSync;
using QSB.EchoesOfTheEye.EclipseElevators.WorldObjects;
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

internal class EclipseElevatorManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public static readonly List<EclipseElevatorController> Elevators = new();

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		Elevators.Clear();
		Elevators.AddRange(QSBWorldSync.GetUnityObjects<EclipseElevatorController>().SortDeterministic());
		QSBWorldSync.Init<QSBEclipseElevatorController, EclipseElevatorController>();

		var allElevators = QSBWorldSync.GetWorldObjects<QSBEclipseElevatorController>().ToArray();

		if (QSBCore.IsHost)
		{
			foreach (var item in allElevators)
			{
				var networkObject = Instantiate(QSBNetworkManager.singleton.ElevatorPrefab);
				networkObject.SpawnWithServerAuthority();
			}
		}

		await UniTask.WaitUntil(() => EclipseDoorVariableSyncer.GetSpecificSyncers<EclipseElevatorVariableSyncer>().Count == Elevators.Count, cancellationToken: ct);

		foreach (var item in allElevators)
		{
			var index = Elevators.IndexOf(item.AttachedObject);
			var syncer = EclipseDoorVariableSyncer.GetSpecificSyncers<EclipseElevatorVariableSyncer>()[index];
			item.SetSyncer(syncer);
		}
	}
}
