using Cysharp.Threading.Tasks;
using QSB.Anglerfish.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.Anglerfish;

public class AnglerManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) =>
		QSBWorldSync.Init<QSBAngler, AnglerfishController>();
}
