using Cysharp.Threading.Tasks;
using QSB.WorldSync;
using QSB.ZeroGCaveSync.WorldObjects;
using System.Threading;

namespace QSB.ZeroGCaveSync;

public class ZeroGCaveManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
		=> QSBWorldSync.Init<QSBSatelliteNode, SatelliteNode>();
}