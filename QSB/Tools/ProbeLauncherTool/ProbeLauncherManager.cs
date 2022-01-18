using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.WorldSync;

namespace QSB.Tools.ProbeLauncherTool
{
	internal class ProbeLauncherManager : WorldObjectManager
	{
		public override WorldObjectType WorldObjectType => WorldObjectType.Both;

		public override void BuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBProbeLauncher, ProbeLauncher>(typeof(PlayerProbeLauncher));
	}
}