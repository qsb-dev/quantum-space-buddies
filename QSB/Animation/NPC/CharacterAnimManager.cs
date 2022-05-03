using Cysharp.Threading.Tasks;
using QSB.Animation.NPC.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.Animation.NPC;

internal class CharacterAnimManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		QSBWorldSync.Init<QSBCharacterAnimController, CharacterAnimController>();
		QSBWorldSync.Init<QSBTravelerController, TravelerController>();
		QSBWorldSync.Init<QSBSolanumController, NomaiConversationManager>();
		QSBWorldSync.Init<QSBSolanumAnimController, SolanumAnimController>();
		QSBWorldSync.Init<QSBHearthianRecorderEffects, HearthianRecorderEffects>();
		QSBWorldSync.Init<QSBTravelerEyeController, TravelerEyeController>();
	}
}