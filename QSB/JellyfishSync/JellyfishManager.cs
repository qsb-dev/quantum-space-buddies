using QSB.JellyfishSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.JellyfishSync
{
	public class JellyfishManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBJellyfish, JellyfishController>();
	}
}
