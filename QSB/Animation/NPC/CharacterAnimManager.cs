using Cysharp.Threading.Tasks;
using QSB.Animation.NPC.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.Animation.NPC;

public class CharacterAnimManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) => 
		QSBWorldSync.Init<QSBSolanumAnimController, SolanumAnimController>();
}