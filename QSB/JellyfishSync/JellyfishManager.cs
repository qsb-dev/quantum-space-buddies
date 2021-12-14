using System.Collections.Generic;
using QSB.JellyfishSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.JellyfishSync
{
	public class JellyfishManager : WorldObjectManager
	{
		public static readonly List<JellyfishController> Jellyfish = new();

		protected override void RebuildWorldObjects(OWScene scene)
		{
			Jellyfish.Clear();
			Jellyfish.AddRange(QSBWorldSync.GetUnityObjects<JellyfishController>());
			QSBWorldSync.Init<QSBJellyfish, JellyfishController>();
		}
	}
}
