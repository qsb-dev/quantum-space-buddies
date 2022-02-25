using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.AirlockSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.AirlockSync;

internal class AirlockManager : WorldObjectManager
{
	// is this used in the prisoner sequence in the eye?
	public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) => QSBWorldSync.Init<QSBGhostAirlock, GhostAirlock>();
}