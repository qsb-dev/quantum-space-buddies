using QSB.Animation.Character.WorldObjects;
using QSB.WorldSync;

namespace QSB.Animation.Character
{
	internal class CharacterAnimManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBCharacterAnimController, CharacterAnimController>();
	}
}
