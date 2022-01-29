using Cysharp.Threading.Tasks;
using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.WorldSync;
using System.Linq;
using System.Threading;

namespace QSB.Tools.ProbeLauncherTool
{
	internal class ProbeLauncherManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		public override async UniTask BuildWorldObjects(OWScene scene, CancellationToken cancellationToken)
		{
			QSBWorldSync.Init<QSBProbeLauncher, ProbeLauncher>(typeof(PlayerProbeLauncher));
			if (scene == OWScene.SolarSystem)
			{
				QSBWorldSync.Init<QSBProbeLauncher, ProbeLauncher>(new[]
				{
					QSBWorldSync.GetUnityObjects<ShipCockpitController>().First().GetShipProbeLauncher()
				});
			}
		}
	}
}