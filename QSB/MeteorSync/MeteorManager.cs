using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;
using System.Linq;

namespace QSB.MeteorSync
{
	public class MeteorManager : WorldObjectManager
	{
		public static WhiteHoleVolume WhiteHoleVolume;

		protected override void RebuildWorldObjects(OWScene scene)
		{
			// wait for all late initializers (which includes meteor launchers) to finish
			StartDelayedReady();
			QSBCore.UnityEvents.RunWhen(() => LateInitializerManager.s_lateInitializers.Count == 0, () =>
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
