﻿using Cysharp.Threading.Tasks;
using QSB.TornadoSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.TornadoSync;

public class TornadoManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
		=> QSBWorldSync.Init<QSBTornado, TornadoController>();
}