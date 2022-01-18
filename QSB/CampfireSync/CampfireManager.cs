using QSB.CampfireSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.CampfireSync
{
	internal class CampfireManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		public override void BuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBCampfire, Campfire>();
	}
}
