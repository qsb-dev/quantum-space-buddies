using Cysharp.Threading.Tasks;
using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.MeteorSync;

public class MeteorManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public static WhiteHoleVolume WhiteHoleVolume;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		// wait for all late initializers (which includes meteor launchers) to finish
		await UniTask.WaitUntil(() => LateInitializerManager.isDoneInitializing, cancellationToken: ct);

		WhiteHoleVolume = QSBWorldSync.GetUnityObject<WhiteHoleVolume>();
		QSBWorldSync.Init<QSBFragment, FragmentIntegrity>();
		QSBWorldSync.Init<QSBMeteorLauncher, MeteorLauncher>();
		QSBWorldSync.Init<QSBMeteor, MeteorController>();
	}
}
