using System.Linq;
using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.MeteorSync
{
	public class MeteorManager : WorldObjectManager
	{
		public static bool Ready;
		public static WhiteHoleVolume WhiteHoleVolume;

		protected override void RebuildWorldObjects(OWScene scene)
		{
			// wait for all late initializers (which includes meteor launchers) to finish
			QSBCore.UnityEvents.RunWhen(() => LateInitializerManager.s_lateInitializers.Count == 0, () =>
			{
				WhiteHoleVolume = QSBWorldSync.GetUnityObjects<WhiteHoleVolume>().First();
				QSBWorldSync.Init<QSBMeteorLauncher, MeteorLauncher>();
				QSBWorldSync.Init<QSBMeteor, MeteorController>();
				QSBWorldSync.Init<QSBFragment, FragmentIntegrity>();
				Ready = true;
			});
		}
	}
}
