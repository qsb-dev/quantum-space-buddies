using QSB.GeyserSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.GeyserSync
{
	public class GeyserManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBGeyser, GeyserController>();
	}
}