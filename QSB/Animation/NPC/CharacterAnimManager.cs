using QSB.Animation.NPC.WorldObjects;
using QSB.WorldSync;
using System.Linq;
using UnityEngine;

namespace QSB.Animation.NPC
{
	internal class CharacterAnimManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBCharacterAnimController, CharacterAnimController>();
			QSBWorldSync.Init<QSBTravelerController, TravelerController>();

			//TODO : this is the wrong place to put this... move it to Conversations?
			QSBWorldSync.OldDialogueTrees.Clear();
			QSBWorldSync.OldDialogueTrees = Resources.FindObjectsOfTypeAll<CharacterDialogueTree>().ToList();
		}
	}
}
