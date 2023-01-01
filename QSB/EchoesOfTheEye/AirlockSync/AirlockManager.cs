using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.AirlockSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.AirlockSync;

internal class AirlockManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		QSBWorldSync.Init<QSBAirlockInterface, AirlockInterface>();
		QSBWorldSync.Init<QSBGhostAirlock, GhostAirlock>();
	}
}
