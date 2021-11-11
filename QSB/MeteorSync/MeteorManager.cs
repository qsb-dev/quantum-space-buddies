using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.MeteorSync
{
	/// we have to do this fake bs
	public class MeteorManager : WorldObjectManager
	{
		public new static bool AllReady;

		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBMeteorLauncher, MeteorLauncher>();
			// wait a bit because meteors get created late
			QSBCore.UnityEvents.FireInNUpdates(() =>
			{
				QSBWorldSync.Init<QSBMeteor, MeteorController>();
				AllReady = true;
			}, 10);
		}
	}
}
