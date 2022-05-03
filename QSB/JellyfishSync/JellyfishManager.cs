using Cysharp.Threading.Tasks;
using QSB.JellyfishSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.JellyfishSync;

public class JellyfishManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) =>
		QSBWorldSync.Init<QSBJellyfish, JellyfishController>();
}
