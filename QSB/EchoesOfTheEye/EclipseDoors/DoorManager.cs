using Cysharp.Threading.Tasks;
using Mirror;
using QSB.EchoesOfTheEye.EclipseDoors.VariableSync;
using QSB.EchoesOfTheEye.EclipseDoors.WorldObjects;
using QSB.Utility;
using QSB.Utility.VariableSync;
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
		await QSBWorldSync.InitWithVariableSync<QSBEclipseDoorController, EclipseDoorController, EclipseDoorVariableSyncer>(ct);
	}
}
