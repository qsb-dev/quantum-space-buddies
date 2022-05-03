using Cysharp.Threading.Tasks;
using QSB.OrbSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.OrbSync;

public class OrbManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) =>
		QSBWorldSync.Init<QSBOrb, NomaiInterfaceOrb>();
}
