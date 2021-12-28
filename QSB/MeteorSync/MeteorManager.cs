using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;
using System.Linq;

namespace QSB.MeteorSync
{
	public class MeteorManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		public static WhiteHoleVolume WhiteHoleVolume;

		protected override void RebuildWorldObjects(OWScene scene)
		{
			// wait for all late initializers (which includes meteor launchers) to finish
			StartDelayedReady();
			QSBCore.UnityEvents.RunWhen(() => LateInitializerManager.isDoneInitializing, () =>
			{
				FinishDelayedReady();
				WhiteHoleVolume = QSBWorldSync.GetUnityObjects<WhiteHoleVolume>().First();
				QSBWorldSync.Init<QSBMeteorLauncher, MeteorLauncher>(this);
				QSBWorldSync.Init<QSBMeteor, MeteorController>(this);
				QSBWorldSync.Init<QSBFragment, FragmentIntegrity>(this);
			});
		}
	}
}
