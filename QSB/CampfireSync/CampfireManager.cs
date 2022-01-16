using QSB.CampfireSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.CampfireSync
{
	internal class CampfireManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		protected override void RebuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBCampfire, Campfire>();
	}
}
