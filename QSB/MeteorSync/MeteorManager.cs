using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.MeteorSync
{
	public class MeteorManager : WorldObjectManager
	{
		public static bool MeteorsReady;

		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBMeteorLauncher, MeteorLauncher>();
			QSBWorldSync.Init<QSBFragment, FragmentIntegrity>();
			// wait a bit because meteors get created late
			QSBCore.UnityEvents.FireInNUpdates(() =>
			{
				QSBWorldSync.Init<QSBMeteor, MeteorController>();
				MeteorsReady = true;
			}, 10);
		}
	}
}
