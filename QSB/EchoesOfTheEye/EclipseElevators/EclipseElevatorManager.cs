using Cysharp.Threading.Tasks;
using QSB.EchoesOfTheEye.EclipseElevators.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.EchoesOfTheEye.EclipseElevators;

public class EclipseElevatorManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;
	public override bool DlcOnly => true;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		QSBWorldSync.Init<QSBEclipseElevatorController, EclipseElevatorController>();
		QSBWorldSync.Init<QSBElevatorDestination, ElevatorDestination>();
	}
}
