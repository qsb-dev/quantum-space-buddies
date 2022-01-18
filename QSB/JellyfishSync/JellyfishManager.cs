using QSB.JellyfishSync.WorldObjects;
using QSB.WorldSync;
using System.Collections.Generic;

namespace QSB.JellyfishSync
{
	public class JellyfishManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		public static readonly List<JellyfishController> Jellyfish = new();

		public override void BuildWorldObjects(OWScene scene)
		{
			Jellyfish.Clear();
			Jellyfish.AddRange(QSBWorldSync.GetUnityObjects<JellyfishController>());
			QSBWorldSync.Init<QSBJellyfish, JellyfishController>(Jellyfish);
		}
	}
}
