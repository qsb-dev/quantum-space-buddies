using Cysharp.Threading.Tasks;
using QSB.StationaryProbeLauncherSync.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Threading;

namespace QSB.StationaryProbeLauncherSync;

public class StationaryProbeLauncherManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.SolarSystem;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct) =>
		// Using ProbeLaunchers here so we can do inheritance, put only applying it to found StationaryProbeLauncher
		QSBWorldSync.Init<QSBStationaryProbeLauncher, ProbeLauncher>(QSBWorldSync.GetUnityObjects<StationaryProbeLauncher>().SortDeterministic());
}
