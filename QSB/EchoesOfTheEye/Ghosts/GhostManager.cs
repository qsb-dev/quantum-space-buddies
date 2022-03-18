using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.Ghosts.WorldObjects;
using QSB.WorldSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QSB.EchoesOfTheEye.Ghosts;

internal class GhostManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		QSBWorldSync.Init<QSBGhostBrain, GhostBrain>();
	}
}
