using Cysharp.Threading.Tasks;
using QSB.CampfireSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.CampfireSync;

public class CampfireManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
		=> QSBWorldSync.Init<QSBCampfire, Campfire>();
}