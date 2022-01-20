using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.WorldSync;
using System.Linq;

namespace QSB.Tools.ProbeLauncherTool
{
	internal class ProbeLauncherManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		public override void BuildWorldObjects(OWScene scene)
		{
			QSBWorldSync.Init<QSBProbeLauncher, ProbeLauncher>(typeof(PlayerProbeLauncher));
			QSBWorldSync.Init<QSBProbeLauncher, ProbeLauncher>(new[]
			{
				QSBWorldSync.GetUnityObjects<ShipCockpitController>().First().GetShipProbeLauncher()
			});
		}
	}
}