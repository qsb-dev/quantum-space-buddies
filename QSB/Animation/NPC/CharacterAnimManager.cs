using Cysharp.Threading.Tasks;
using QSB.Animation.NPC.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.Animation.NPC
{
	internal class CharacterAnimManager : WorldObjectManager
	{
		// im assuming this is used in the eye as well
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken cancellationToken)
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
