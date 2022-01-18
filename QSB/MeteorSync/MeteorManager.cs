using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;
using System.Linq;

namespace QSB.MeteorSync
{
	public class MeteorManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		public static WhiteHoleVolume WhiteHoleVolume;

		public override void BuildWorldObjects(OWScene scene)
		{
			// wait for all late initializers (which includes meteor launchers) to finish
			StartDelayedReady();
			QSBCore.UnityEvents.RunWhen(() => LateInitializerManager.isDoneInitializing, () =>
			{
				FinishDelayedReady();
				WhiteHoleVolume = QSBWorldSync.GetUnityObjects<WhiteHoleVolume>().First();
				QSBWorldSync.Init<QSBMeteorLauncher, MeteorLauncher>();
				QSBWorldSync.Init<QSBMeteor, MeteorController>();
				QSBWorldSync.Init<QSBFragment, FragmentIntegrity>();
			});
		}
	}
}
