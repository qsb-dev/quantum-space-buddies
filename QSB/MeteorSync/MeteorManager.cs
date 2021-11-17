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
			// wait a bit because meteors get created late
			QSBCore.UnityEvents.FireInNUpdates(() =>
			{
				WhiteHoleVolume = QSBWorldSync.GetUnityObjects<WhiteHoleVolume>().First();
				QSBWorldSync.Init<QSBMeteorLauncher, MeteorLauncher>();
				QSBWorldSync.Init<QSBMeteor, MeteorController>();
				QSBWorldSync.Init<QSBFragment, FragmentIntegrity>();
				Ready = true;
			}, 50);
		}
	}
}
