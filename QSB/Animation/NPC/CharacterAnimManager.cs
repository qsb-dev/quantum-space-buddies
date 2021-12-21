using QSB.Animation.NPC.WorldObjects;
using QSB.WorldSync;
using System.Linq;

namespace QSB.Animation.NPC
{
	internal class CharacterAnimManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBCharacterAnimController, CharacterAnimController>();
			QSBWorldSync.Init<QSBTravelerController, TravelerController>();
			QSBWorldSync.Init<QSBSolanumController, NomaiConversationManager>();
			QSBWorldSync.Init<QSBSolanumAnimController, SolanumAnimController>();
			QSBWorldSync.Init<QSBHearthianRecorderEffects, HearthianRecorderEffects>();
			QSBWorldSync.Init<QSBTravelerEyeController, TravelerEyeController>();

			//MOVE : this is the wrong place to put this... move it to Conversations?
			QSBWorldSync.OldDialogueTrees.Clear();
			QSBWorldSync.OldDialogueTrees.AddRange(QSBWorldSync.GetUnityObjects<CharacterDialogueTree>());
		}
	}
}
