using System.Linq;
using QSB.MeteorSync.WorldObjects;
using QSB.WorldSync;
using UnityEngine;

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
				QSBWorldSync.Init<QSBMeteorLauncher, MeteorLauncher>();
				QSBWorldSync.Init<QSBMeteor, MeteorController>();
				QSBWorldSync.Init<QSBFragment, FragmentIntegrity>();
				WhiteHoleVolume = Resources.FindObjectsOfTypeAll<WhiteHoleVolume>().First();
				Ready = true;
			}, 10);
		}
	}
}
