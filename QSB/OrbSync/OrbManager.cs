using Cysharp.Threading.Tasks;
using QSB.OrbSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using System.Threading;

namespace QSB.OrbSync;

public class OrbManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) =>
		// NH sometimes makes the body (BUT NOT THE ORB) null SOMEHOW!!!!!!
		QSBWorldSync.Init<QSBOrb, NomaiInterfaceOrb>(QSBWorldSync.GetUnityObjects<NomaiInterfaceOrb>()
			.Where(x => x.GetAttachedOWRigidbody())
			.SortDeterministic());
}
