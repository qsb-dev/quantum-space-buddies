using Cysharp.Threading.Tasks;
using QSB.ElevatorSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.ElevatorSync
{
	public class ElevatorManager : WorldObjectManager
	{
		public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

		public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
			=> QSBWorldSync.Init<QSBElevator, Elevator>();
	}
}