using QSB.Animation.NPC.WorldObjects;
using QSB.WorldSync;

namespace QSB.Animation.NPC
{
	internal class CharacterAnimManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBCharacterAnimController, CharacterAnimController>();
			QSBWorldSync.Init<QSBTravelerController, TravelerController>();
		}
	}
}
