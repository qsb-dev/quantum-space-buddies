using Cysharp.Threading.Tasks;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.Utility;
using QSB.WorldSync;
using System.Linq;
using System.Threading;

namespace QSB.Tools.ProbeLauncherTool;

public class ProbeLauncherManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		QSBWorldSync.Init<QSBProbeLauncher, ProbeLauncher>(typeof(PlayerProbeLauncher), typeof(StationaryProbeLauncher));
		// Using ProbeLaunchers here so we can do inheritance, put only applying it to found StationaryProbeLauncher
		QSBWorldSync.Init<QSBStationaryProbeLauncher, ProbeLauncher>(QSBWorldSync.GetUnityObjects<StationaryProbeLauncher>().SortDeterministic());
		if (scene == OWScene.SolarSystem)
		{
			QSBWorldSync.Init<QSBProbeLauncher, ProbeLauncher>(new[]
			{
				QSBWorldSync.GetUnityObjects<ShipCockpitController>().First().GetShipProbeLauncher()
			});
		}
	}
}