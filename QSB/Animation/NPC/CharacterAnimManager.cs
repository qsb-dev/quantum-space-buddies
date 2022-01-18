using QSB.Animation.NPC.WorldObjects;
using QSB.WorldSync;

namespace QSB.Animation.NPC
{
	internal class CharacterAnimManager : WorldObjectManager
	{
		// im assuming this is used in the eye as well
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		public override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBCharacterAnimController, CharacterAnimController>();
			QSBWorldSync.Init<QSBTravelerController, TravelerController>();
			QSBWorldSync.Init<QSBSolanumController, NomaiConversationManager>();
			QSBWorldSync.Init<QSBSolanumAnimController, SolanumAnimController>();
			QSBWorldSync.Init<QSBHearthianRecorderEffects, HearthianRecorderEffects>();
			QSBWorldSync.Init<QSBTravelerEyeController, TravelerEyeController>();
		}
	}
}
