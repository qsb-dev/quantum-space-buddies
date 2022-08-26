using Cysharp.Threading.Tasks;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.WorldSync;
using System.Linq;
using System.Threading;

namespace QSB.Tools.ProbeLauncherTool;

internal class ProbeLauncherManager : WorldObjectManager
{
	public override WorldObjectScene WorldObjectScene => WorldObjectScene.Both;

	public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken ct)
	{
		QSBWorldSync.Init<QSBProbeLauncher, ProbeLauncher>(typeof(PlayerProbeLauncher), typeof(StationaryProbeLauncher));
		if (scene == OWScene.SolarSystem)
		{
			QSBWorldSync.Init<QSBProbeLauncher, ProbeLauncher>(new[]
			{
				QSBWorldSync.GetUnityObjects<ShipCockpitController>().First().GetShipProbeLauncher()
			});
		}
	}
}