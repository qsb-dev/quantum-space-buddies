using QSB.Animation.NPC.WorldObjects;
using QSB.WorldSync;

namespace QSB.Animation.NPC
{
	internal class CharacterAnimManager : WorldObjectManager
	{
		// im assuming this is used in the eye as well
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBCharacterAnimController, CharacterAnimController>(this);
			QSBWorldSync.Init<QSBTravelerController, TravelerController>(this);
			QSBWorldSync.Init<QSBSolanumController, NomaiConversationManager>(this);
			QSBWorldSync.Init<QSBSolanumAnimController, SolanumAnimController>(this);
			QSBWorldSync.Init<QSBHearthianRecorderEffects, HearthianRecorderEffects>(this);
			QSBWorldSync.Init<QSBTravelerEyeController, TravelerEyeController>(this);

			//MOVE : this is the wrong place to put this... move it to Conversations?
			QSBWorldSync.OldDialogueTrees.Clear();
			QSBWorldSync.OldDialogueTrees.AddRange(QSBWorldSync.GetUnityObjects<CharacterDialogueTree>());
		}
	}
}
