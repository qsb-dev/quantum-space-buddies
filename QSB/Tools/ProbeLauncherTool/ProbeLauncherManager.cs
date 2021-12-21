using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.WorldSync;

namespace QSB.Tools.ProbeLauncherTool
{
	internal class ProbeLauncherManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.SolarSystem;

		protected override void RebuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBProbeLauncher, ProbeLauncher>(typeof(PlayerProbeLauncher));
	}
}