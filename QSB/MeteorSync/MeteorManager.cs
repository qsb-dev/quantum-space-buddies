using Cysharp.Threading.Tasks;
using QSB.MeteorSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
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

		WhiteHoleVolume = QSBWorldSync.GetUnityObjects<WhiteHoleVolume>().First();
		QSBWorldSync.Init<QSBFragment, FragmentIntegrity>();

		var meteorLaunchers = QSBWorldSync.GetUnityObjects<MeteorLauncher>().SortDeterministic().ToList();
		QSBWorldSync.Init<QSBMeteorLauncher, MeteorLauncher>(meteorLaunchers);

		// order by pool instead of using SortDeterministic
		var meteors = meteorLaunchers.SelectMany(x =>
			x._meteorPool.EmptyIfNull().Concat(x._dynamicMeteorPool.EmptyIfNull()));
		QSBWorldSync.Init<QSBMeteor, MeteorController>(meteors);
	}
}
