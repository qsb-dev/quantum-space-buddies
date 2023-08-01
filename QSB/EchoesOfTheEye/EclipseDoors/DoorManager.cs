using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.EclipseDoors.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.EclipseDoors;

public class DoorManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) =>
		QSBWorldSync.Init<QSBEclipseDoorController, EclipseDoorController>();
}
