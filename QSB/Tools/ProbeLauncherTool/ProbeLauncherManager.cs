using QSB.Tools.ProbeLauncherTool.WorldObjects;
using QSB.WorldSync;

namespace QSB.Tools.ProbeLauncherTool
{
	class ProbeLauncherManager : WorldObjectManager
	{
		protected override void RebuildWorldObjects(OWScene scene)
			=> QSBWorldSync.Init<QSBProbeLauncher, ProbeLauncher>();
	}
}