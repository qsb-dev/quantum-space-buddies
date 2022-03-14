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

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		await QSBWorldSync.InitWithVariableSync<QSBEclipseElevatorController, EclipseElevatorController, EclipseElevatorVariableSyncer>(ct, QSBNetworkManager.singleton.ElevatorPrefab);
	}
}
