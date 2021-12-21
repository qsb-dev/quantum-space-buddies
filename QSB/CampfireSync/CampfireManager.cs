using QSB.CampfireSync.WorldObjects;
using QSB.WorldSync;

namespace QSB.CampfireSync
{
	internal class CampfireManager : WorldObjectManager
	{
		// is this needed for the campfire in the eye? or is that a special one?
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		protected override void RebuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBCampfire, Campfire>();
	}
}
