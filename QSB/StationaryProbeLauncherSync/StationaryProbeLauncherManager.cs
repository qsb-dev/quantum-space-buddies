using Cysharp.Threading.Tasks;
using QSB.StationaryProbeLauncherSync.WorldObjects;
using QSB.WorldSync;
using System.Threading;

namespace QSB.StationaryProbeLauncherSync;

public class StationaryProbeLauncherManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) =>
		QSBWorldSync.Init<QSBStationaryProbeLauncher, ProbeLauncher>();
}
