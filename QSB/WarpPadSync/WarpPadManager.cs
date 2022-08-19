using Cysharp.Threading.Tasks;
using QSB.WarpPadSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.WarpPadSync;

public class WarpPadManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) =>
		QSBWorldSync.Init<QSBWarpPad, NomaiWarpPlatform>();
}
