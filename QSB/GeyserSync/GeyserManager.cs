using QSB.GeyserSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.GeyserSync
{
	public class GeyserManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		public override void BuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBGeyser, GeyserController>();
	}
}