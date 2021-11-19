using System.Linq;
using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.MeteorSync
{
	public class MeteorManager : WorldObjectManager
	{
		public static bool Ready => AllReady && _ready;
		private static bool _ready;
		public static WhiteHoleVolume WhiteHoleVolume;

		protected override void RebuildWorldObjects(OWScene scene)
		{
			_ready = false;
			// wait for all late initializers (which includes meteor launchers) to finish
			QSBCore.UnityEvents.RunWhen(() => LateInitializerManager.s_lateInitializers.Count == 0, () =>
			{
				WhiteHoleVolume = QSBWorldSync.GetUnityObjects<WhiteHoleVolume>().First();
				QSBWorldSync.Init<QSBMeteorLauncher, MeteorLauncher>();
				QSBWorldSync.Init<QSBMeteor, MeteorController>();
				QSBWorldSync.Init<QSBFragment, FragmentIntegrity>();
				_ready = true;
			});
		}
	}
}
